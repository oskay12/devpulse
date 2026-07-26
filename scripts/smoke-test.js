const baseUrl = process.env.API_URL || "http://k8s-devpulse-devpulse-3b71e8f78f-1851517318.us-east-1.elb.amazonaws.com";

const colors = {
  green: "\x1b[32m",
  red: "\x1b[31m",
  cyan: "\x1b[36m",
  yellow: "\x1b[33m",
  reset: "\x1b[0m"
};

function logStep(stepNum, message) {
  console.log(`${colors.cyan}[Step ${stepNum}]${colors.reset} ${message}`);
}

function logSuccess(message) {
  console.log(`  ${colors.green}✔ ${message}${colors.reset}`);
}

function logError(message) {
  console.log(`  ${colors.red}✖ ${message}${colors.reset}`);
}

async function request(path, options = {}) {
  const url = `${baseUrl}${path}`;
  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  const contentType = response.headers.get("content-type") || "";
  let body = null;
  if (contentType.includes("application/json")) {
    body = await response.json();
  } else {
    body = await response.text();
  }

  return { status: response.status, body, headers: response.headers };
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function runSmokeTest() {
  console.log(`\n🚀 ${colors.yellow}Starting DevPulse E2E Smoke Test Suite${colors.reset}`);
  console.log(`📍 Target Base URL: ${baseUrl}\n`);

  const timestamp = Date.now();
  const username = `smoke_user_${timestamp}`;
  const email = `smoke_${timestamp}@example.com`;
  const repoName = `smoke_repo_${timestamp}`;
  const fullName = `smoke-org/${repoName}`;
  const externalId = `ext_${timestamp}`;

  try {
    logStep(1, "Checking Health Endpoints (/health/live & /health/ready)");
    const liveRes = await request("/health/live");
    if (liveRes.status !== 200) throw new Error(`Liveness failed with status ${liveRes.status}`);
    const readyRes = await request("/health/ready");
    if (readyRes.status !== 200) throw new Error(`Readiness failed with status ${readyRes.status}`);
    logSuccess("Health checks passed (200 OK)");

    logStep(2, `Creating User (${username})`);
    const createUserRes = await request("/api/users", {
      method: "POST",
      body: JSON.stringify({
        username,
        email,
        password: "SecurePassword123!",
        avatar_url: "https://example.com/avatar.png",
        role: "developer"
      })
    });
    if (createUserRes.status !== 201) throw new Error(`Create user failed (${createUserRes.status}): ${JSON.stringify(createUserRes.body)}`);
    const userId = createUserRes.body.id;
    logSuccess(`User created with ID: ${userId}`);

    logStep(3, `Registering Repository (${fullName})`);
    const createRepoRes = await request("/api/repositories", {
      method: "POST",
      body: JSON.stringify({
        name: repoName,
        full_name: fullName,
        description: "Automated end-to-end smoke test repository",
        clone_url: `https://github.com/${fullName}.git`,
        default_branch: "main",
        provider: "gitlab",
        external_id: externalId,
        owner_id: userId,
        is_private: false,
        star_count: 10,
        fork_count: 2
      })
    });
    if (createRepoRes.status !== 201) throw new Error(`Create repository failed (${createRepoRes.status}): ${JSON.stringify(createRepoRes.body)}`);
    const repositoryId = createRepoRes.body.id;
    logSuccess(`Repository registered with ID: ${repositoryId}`);

    logStep(4, "Issuing Project Token for Webhook Authentication");
    const createTokenRes = await request(`/api/repositories/${repositoryId}/tokens`, {
      method: "POST",
      body: JSON.stringify({
        name: "Smoke Test Webhook Token",
        permissions: "WriteWebhooks"
      })
    });
    if (createTokenRes.status !== 201) throw new Error(`Create token failed (${createTokenRes.status}): ${JSON.stringify(createTokenRes.body)}`);
    const projectToken = createTokenRes.body.token;
    logSuccess("Project token issued successfully");

    logStep(5, "Sending Simulated GitLab Push Webhook Event");
    const commitSha = `sha_${timestamp}`;
    const commitMsg = `feat: smoke test automated ingestion ${timestamp}`;
    const webhookRes = await request("/api/webhooks/gitlab", {
      method: "POST",
      headers: {
        "X-Gitlab-Token": projectToken,
        "X-Gitlab-Event": "Push Hook"
      },
      body: JSON.stringify({
        object_kind: "push",
        ref: "refs/heads/main",
        commits: [
          {
            id: commitSha,
            message: commitMsg,
            timestamp: new Date().toISOString(),
            author: {
              name: username,
              email: email
            },
            added: ["src/SmokeTest.cs"],
            modified: [],
            removed: []
          }
        ]
      })
    });
    if (webhookRes.status !== 202) throw new Error(`Webhook failed (${webhookRes.status}): ${JSON.stringify(webhookRes.body)}`);
    logSuccess(`Webhook accepted (202 Accepted, Event ID: ${webhookRes.body.event_id})`);

    logStep(6, "Waiting for Background Queue Processing (RabbitMQ -> Worker -> PostgreSQL)...");
    await sleep(3500);

    logStep(7, "Verifying Commit Ingestion in Database");
    const commitsRes = await request(`/api/repositories/${repositoryId}/commits`);
    if (commitsRes.status !== 200) throw new Error(`List commits failed (${commitsRes.status})`);
    const items = commitsRes.body.items || [];
    const foundCommit = items.find((c) => c.message === commitMsg || c.sha === commitSha);
    if (!foundCommit) throw new Error(`Commit ${commitSha} was not found in repository commits list`);
    logSuccess(`Commit ingested successfully in PostgreSQL (ID: ${foundCommit.id})`);

    logStep(8, "Triggering OpenSearch Reindexing");
    const reindexRes = await request(`/api/search/reindex?contentType=commit&repositoryId=${repositoryId}`, {
      method: "POST"
    });
    if (reindexRes.status !== 202) throw new Error(`Reindex failed (${reindexRes.status})`);
    logSuccess("Reindex job queued in OpenSearch");

    logStep(9, "Waiting for OpenSearch Indexing...");
    await sleep(3500);

    logStep(10, "Verifying Full-Text Search via OpenSearch");
    const searchRes = await request(`/api/search/commits?q=smoke&repositoryId=${repositoryId}`);
    if (searchRes.status !== 200) throw new Error(`Search failed (${searchRes.status})`);
    const searchHits = searchRes.body.items || searchRes.body.data || [];
    logSuccess(`OpenSearch query executed successfully (${searchHits.length} match(es) found)`);

    console.log(`\n🎉 ${colors.green}ALL SMOKE TESTS PASSED SUCCESSFULLY!${colors.reset}\n`);
    process.exit(0);
  } catch (err) {
    logError(err.message);
    console.log(`\n💥 ${colors.red}SMOKE TEST FAILED!${colors.reset}\n`);
    process.exit(1);
  }
}

runSmokeTest();
