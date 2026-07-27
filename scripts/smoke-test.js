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

function logWarn(message) {
  console.log(`  ${colors.yellow}⟳ ${message}${colors.reset}`);
}

// Transient statuses returned by the load balancer while a freshly rolled-out
// pod is still warming up (not yet registered/healthy in the ELB target group).
// These are retried; genuine app errors (4xx/500) are NOT — they surface at once.
const RETRYABLE_STATUSES = new Set([502, 503, 504]);
const MAX_ATTEMPTS = 8;
const BASE_BACKOFF_MS = 1000;
const MAX_BACKOFF_MS = 8000;

async function request(path, options = {}) {
  const url = `${baseUrl}${path}`;

  let lastError = null;
  for (let attempt = 1; attempt <= MAX_ATTEMPTS; attempt++) {
    let response;
    try {
      response = await fetch(url, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          ...(options.headers || {})
        }
      });
    } catch (err) {
      // Network-level failure (connection reset/refused during rollout) — retry.
      lastError = err;
      if (attempt < MAX_ATTEMPTS) {
        await sleep(backoffDelay(attempt));
        continue;
      }
      throw err;
    }

    // Retry gateway errors from the ELB while the new pod stabilises.
    if (RETRYABLE_STATUSES.has(response.status) && attempt < MAX_ATTEMPTS) {
      const delay = backoffDelay(attempt);
      logWarn(`${response.status} on ${path} (attempt ${attempt}/${MAX_ATTEMPTS}), retrying in ${delay}ms...`);
      await sleep(delay);
      continue;
    }

    const contentType = response.headers.get("content-type") || "";
    let body = null;
    if (contentType.includes("application/json")) {
      body = await response.json();
    } else {
      body = await response.text();
    }

    return { status: response.status, body, headers: response.headers };
  }

  throw lastError ?? new Error(`Request to ${path} failed after ${MAX_ATTEMPTS} attempts`);
}

// Exponential backoff with a ceiling.
function backoffDelay(attempt) {
  return Math.min(BASE_BACKOFF_MS * 2 ** (attempt - 1), MAX_BACKOFF_MS);
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function waitForCommit(repositoryId, commitSha, commitMsg, maxRetries = 20, delayMs = 1000) {
  for (let i = 1; i <= maxRetries; i++) {
    const commitsRes = await request(`/api/repositories/${repositoryId}/commits`);
    if (commitsRes.status === 200 && commitsRes.body && Array.isArray(commitsRes.body.items)) {
      const items = commitsRes.body.items;
      const found = items.find((c) => c.message === commitMsg || c.sha === commitSha);
      if (found) return found;
    }
    await sleep(delayMs);
  }
  return null;
}

async function waitForSearchHits(repositoryId, maxRetries = 20, delayMs = 1000) {
  for (let i = 1; i <= maxRetries; i++) {
    const searchRes = await request(`/api/search/commits?q=smoke&repositoryId=${repositoryId}`);
    if (searchRes.status === 200 && searchRes.body) {
      const items = searchRes.body.items || searchRes.body.data || [];
      if (items.length > 0) return items;
    }
    await sleep(delayMs);
  }
  return [];
}

// Poll /health/ready until it returns 200 several times in a row, so we don't
// start on a single lucky response while the ELB is still shifting traffic to a
// freshly rolled-out pod. request() already retries gateway errors underneath.
async function warmUp(requiredConsecutive = 3, maxRetries = 30, delayMs = 2000) {
  let consecutive = 0;
  for (let i = 1; i <= maxRetries; i++) {
    try {
      const res = await request("/health/ready");
      if (res.status === 200) {
        consecutive++;
        if (consecutive >= requiredConsecutive) return;
      } else {
        consecutive = 0;
      }
    } catch {
      consecutive = 0;
    }
    await sleep(delayMs);
  }
  throw new Error("API did not become stable (health/ready) within the warm-up window");
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
    logStep(0, "Warming up — waiting for API to serve stable traffic after rollout");
    await warmUp();
    logSuccess("API is warm and serving requests");

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

    logStep(6, "Polling for Background Queue Ingestion (RabbitMQ -> Worker -> PostgreSQL)...");
    const foundCommit = await waitForCommit(repositoryId, commitSha, commitMsg, 20, 1000);
    if (!foundCommit) throw new Error(`Commit ${commitSha} was not found in repository commits list after 20 seconds timeout`);
    logSuccess(`Commit ingested successfully in PostgreSQL (ID: ${foundCommit.id})`);

    logStep(7, "Triggering OpenSearch Reindexing");
    const reindexRes = await request(`/api/search/reindex?contentType=commit&repositoryId=${repositoryId}`, {
      method: "POST"
    });
    if (reindexRes.status !== 202) throw new Error(`Reindex failed (${reindexRes.status})`);
    logSuccess("Reindex job queued in OpenSearch");

    logStep(8, "Polling for OpenSearch Full-Text Search Indexing...");
    const searchHits = await waitForSearchHits(repositoryId, 20, 1000);
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
