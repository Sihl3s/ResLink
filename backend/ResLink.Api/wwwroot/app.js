// Browser prototype: same JWT API as Android, with role menus (Microsoft, 2025b; MDN, 2025).
const app = document.getElementById("app");
const state = { user: JSON.parse(localStorage.getItem("reslink.user") || "null"), page: "home", filter: "All", tab: "overview", menu: false, more: false };
let sessionNotice = "";

const navByRole = {
  Student: [
    ["home", "Dashboard"], ["community", "Community"], ["events", "Events"], ["market", "Marketplace"],
    ["groups", "Study Groups"], ["maintenance", "Maintenance"], ["security", "Security"], ["rewards", "Rewards"], ["profile", "Profile"]
  ],
  Admin: [
    ["home", "Dashboard"], ["community", "Community"], ["events", "Events"], ["market", "Marketplace"],
    ["groups", "Study Groups"], ["maintenance", "Maintenance"], ["security", "Security"], ["rewards", "Rewards"],
    ["admin", "Admin Panel"], ["profile", "Profile"]
  ],
  Security: [
    ["home", "Dashboard"], ["security", "Security"], ["notifications", "Notifications"], ["profile", "Profile"]
  ],
  Maintenance: [
    ["home", "Dashboard"], ["maintenance", "Maintenance"], ["notifications", "Notifications"], ["profile", "Profile"]
  ]
};

// Stroke icons share one weight so the chrome does not look illustrated (Material Design, 2025).
function icon(name) {
  const paths = {
    home: '<path d="M4 11.5 12 4l8 7.5"/><path d="M6.5 10.5V20h11V10.5"/>',
    community: '<path d="M5 6.5h14v8H8l-3 3z"/>',
    events: '<rect x="4" y="5" width="16" height="15" rx="2"/><path d="M8 3.5V7M16 3.5V7M4 10h16"/>',
    market: '<path d="M6 8h12l-1 12H7L6 8z"/><path d="M9 8V6.5A3 3 0 0 1 12 3.5 3 3 0 0 1 15 6.5V8"/>',
    groups: '<circle cx="9" cy="9" r="2.2"/><circle cx="15.5" cy="9.5" r="1.8"/><path d="M4.8 18c.4-2.4 2.2-3.6 4.2-3.6s3.8 1.2 4.2 3.6M13 14.6c1.3-.4 2.6-.2 3.6.8 1 1.2 1.3 2.6 1.4 2.6"/>',
    maintenance: '<path d="M14.5 6.5a3.2 3.2 0 0 0-4.2 4.2L4.5 16.5 7.5 19.5l5.8-5.8a3.2 3.2 0 0 0 4.2-4.2L15 12l-3-3 2.5-2.5z"/>',
    security: '<path d="M12 3.5 19 6.5v5.2c0 4.2-2.8 7.2-7 8.8-4.2-1.6-7-4.6-7-8.8V6.5z"/>',
    rewards: '<rect x="4" y="10" width="16" height="9" rx="1.5"/><path d="M12 10v9M4 13.5h16M12 10c-1.4-2.2-4.2-2.4-4.2-.6S11 10 12 10zm0 0c1.4-2.2 4.2-2.4 4.2-.6S13 10 12 10z"/>',
    profile: '<circle cx="12" cy="8.5" r="3"/><path d="M5.5 19.5c1-3 3.2-4.5 6.5-4.5s5.5 1.5 6.5 4.5"/>',
    admin: '<path d="M4 12h16M12 4v16M6.5 6.5l11 11M17.5 6.5l-11 11"/>',
    notifications: '<path d="M6 16.5h12l-1.2-2.2V10a4.8 4.8 0 0 0-9.6 0v4.3z"/><path d="M10 16.5a2 2 0 0 0 4 0"/>',
    more: '<circle cx="6" cy="12" r="1.2"/><circle cx="12" cy="12" r="1.2"/><circle cx="18" cy="12" r="1.2"/>'
  };
  const d = paths[name] || paths.home;
  return `<span class="ico-svg" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">${d}</svg></span>`;
}

class SessionError extends Error {}

async function api(path, options = {}) {
  // Fetch keeps the UI on the same origin as the API (MDN, 2025).
  const headers = { "Content-Type": "application/json", ...(options.headers || {}) };
  if (state.user?.token) headers.Authorization = `Bearer ${state.user.token}`;
  const res = await fetch(path, { ...options, headers });
  // A rejected token after a Render restart must drop the saved session (OWASP, 2023).
  if (res.status === 401 && state.user?.token) {
    save(null);
    sessionNotice = "Your session ended. Sign in again to continue.";
    throw new SessionError("Session expired");
  }
  if (!res.ok) throw new Error((await res.text()) || `Request failed (${res.status})`);
  if (res.status === 204) return null;
  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

const save = user => { state.user = user; user ? localStorage.setItem("reslink.user", JSON.stringify(user)) : localStorage.removeItem("reslink.user"); };
const esc = v => String(v ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
const val = id => document.getElementById(id)?.value ?? "";
const initial = name => (name || "R").trim().charAt(0).toUpperCase();
const tagClass = value => {
  const v = (value || "").toLowerCase();
  if (["social", "student", "good", "electronics"].includes(v)) return "blue";
  if (["sports", "high", "fair"].includes(v)) return "orange";
  if (["academic", "admin", "announcement"].includes(v)) return "purple";
  if (["pending", "open", "medium"].includes(v)) return "yellow";
  if (["resolved", "like new", "member"].includes(v)) return "green";
  return "gray";
};
const prettyStatus = s => s === "Open" ? "Pending" : s === "InProgress" ? "In Progress" : s;
const fmtDate = iso => {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleString(undefined, { weekday: "short", month: "short", day: "numeric", hour: "numeric", minute: "2-digit" });
};

function brand() {
  return `<div class="brand-row"><img src="reslink.jpeg" alt="ResLink" /></div>`;
}

function navButtons(nav) {
  return nav.map(([id, label]) => `<button class="${state.page === id ? "active" : ""}" data-page="${id}">${icon(id)}<span>${label}</span></button>`).join("");
}

async function render() {
  // The sidebar is only shown once a token is stored (OWASP, 2023).
  if (!state.user?.token) {
    if (state.user) save(null);
    app.innerHTML = authHtml();
    bindAuth();
    if (sessionNotice) {
      const note = document.getElementById("auth-error");
      if (note) note.textContent = sessionNotice;
      sessionNotice = "";
    }
    return;
  }
  const nav = navByRole[state.user.role] || navByRole.Student;
  const primary = nav.slice(0, 4);
  const extra = nav.slice(4);
  app.innerHTML = `
    <div class="shell">
      <header class="topbar">
        ${brand()}
        <button class="menu-btn" id="open-menu" aria-label="Open menu">${icon("more")}</button>
      </header>
      ${state.menu ? `<div class="scrim" id="scrim"></div>` : ""}
      <aside class="sidebar${state.menu ? " open" : ""}">
        ${brand()}
        <nav class="nav">${navButtons(nav)}</nav>
        <div class="side-foot">
          <div class="user-line"><div class="avatar">${initial(state.user.fullName)}</div><div class="user-meta"><b>${esc(state.user.fullName)}</b><small>${esc(state.user.role)}</small></div></div>
          <button class="signout" id="logout">Sign out</button>
        </div>
      </aside>
      <main class="main" id="main"></main>
      <nav class="bottom-nav">
        ${primary.map(([id, label]) => `<button class="${state.page === id ? "active" : ""}" data-page="${id}">${icon(id)}<span>${label.split(" ")[0]}</span></button>`).join("")}
        <button id="more-btn" class="${extra.some(([id]) => id === state.page) ? "active" : ""}">${icon("more")}<span>More</span></button>
      </nav>
      ${state.more ? `<div class="more-sheet">${navButtons(extra)}</div>` : ""}
    </div>`;
  document.querySelectorAll("[data-page]").forEach(b => b.onclick = () => { state.page = b.dataset.page; state.filter = "All"; state.menu = false; state.more = false; render(); });
  document.getElementById("logout").onclick = () => { save(null); render(); };
  const open = document.getElementById("open-menu");
  if (open) open.onclick = () => { state.menu = true; state.more = false; render(); };
  const scrim = document.getElementById("scrim");
  if (scrim) scrim.onclick = () => { state.menu = false; render(); };
  document.getElementById("more-btn").onclick = () => { state.more = !state.more; render(); };
  await draw();
}

function authHtml() {
  return `<div class="auth-wrap">
    <section class="auth-hero">
      <img class="auth-mark" src="reslink.jpeg" alt="ResLink" />
      <div><h2>Residence life, in one place.</h2><p>Community, safety, and maintenance for students and staff.</p></div>
    </section>
    <div class="auth-panel"><div class="auth-card">
    <h1>Welcome back</h1>
    <p class="lead">Sign in to open the dashboard for your role.</p>
    <label>Email<input id="email" type="email" value="student@reslink.app" /></label>
    <label>Password<input id="password" type="password" value="Student123!" /></label>
    <p class="error" id="auth-error"></p>
    <p><button class="btn wide" id="login">Log in</button></p>
    <p class="row"><button class="btn ghost" id="show-s">Student sign-up</button><button class="btn ghost" id="show-t">Staff sign-up</button></p>
    <div class="hidden" id="s-box">
      <h3>Student sign-up</h3>
      <label>Full name<input id="s-name" /></label><label>Email<input id="s-email" /></label>
      <label>Password<input id="s-password" type="password" /></label><label>Student number<input id="s-number" /></label>
      <label>Room<input id="s-room" /></label><label>Residence<select id="s-res"></select></label>
      <p class="error" id="s-error"></p><button class="btn" id="reg-s">Create student account</button>
    </div>
    <div class="hidden" id="t-box">
      <h3>Staff sign-up</h3>
      <label>Full name<input id="t-name" /></label><label>Email<input id="t-email" /></label>
      <label>Password<input id="t-password" type="password" /></label><label>Staff ID<input id="t-staff" /></label>
      <label>Access code<input id="t-code" type="password" /></label>
      <label>Role<select id="t-role"><option>Admin</option><option>Security</option><option>Maintenance</option></select></label>
      <label>Residence<select id="t-res"></select></label>
      <p class="error" id="t-error"></p><button class="btn" id="reg-t">Create staff account</button>
    </div>
  </div></div></div>`;
}

function bindAuth() {
  api("/api/auth/residences").then(items => {
    ["s-res", "t-res"].forEach(id => { document.getElementById(id).innerHTML = items.map(r => `<option value="${r.id}">${r.name}</option>`).join(""); });
  }).catch(() => {});
  document.getElementById("login").onclick = async () => {
    try { save(await api("/api/auth/login", { method: "POST", body: JSON.stringify({ email: val("email"), password: val("password") }) })); state.page = "home"; render(); }
    catch { document.getElementById("auth-error").textContent = "Invalid email or password."; }
  };
  document.getElementById("show-s").onclick = () => document.getElementById("s-box").classList.toggle("hidden");
  document.getElementById("show-t").onclick = () => document.getElementById("t-box").classList.toggle("hidden");
  document.getElementById("reg-s").onclick = async () => {
    try { save(await api("/api/auth/register/student", { method: "POST", body: JSON.stringify({ fullName: val("s-name"), email: val("s-email"), password: val("s-password"), studentNumber: val("s-number"), room: val("s-room"), residenceId: val("s-res") }) })); render(); }
    catch (e) { document.getElementById("s-error").textContent = e.message; }
  };
  document.getElementById("reg-t").onclick = async () => {
    try { save(await api("/api/auth/register/staff", { method: "POST", body: JSON.stringify({ fullName: val("t-name"), email: val("t-email"), password: val("t-password"), staffId: val("t-staff"), accessCode: val("t-code"), role: val("t-role"), residenceId: val("t-res") }) })); render(); }
    catch { document.getElementById("t-error").textContent = "Invalid access code or details."; }
  };
}

async function draw() {
  const main = document.getElementById("main");
  try {
    const pages = { home, community, events, market, groups, maintenance, security, rewards, admin, profile, notifications };
    await (pages[state.page] || home)(main);
  } catch (e) {
    if (e instanceof SessionError || !state.user?.token) { render(); return; }
    if (main) main.innerHTML = `<h1>Unable to load</h1><p class="error">${esc(e.message)}</p>`;
  }
}

function greeting() {
  const h = new Date().getHours();
  return h < 12 ? "Good morning" : h < 17 ? "Good afternoon" : "Good evening";
}

// Every role loads announcements; Security/Maintenance skip the community feed shortcuts.
async function home(main) {
  const dash = await api("/api/dashboard");
  const events = await api("/api/events").catch(() => []);
  const posts = await api("/api/posts").catch(() => []);
  const announcements = posts.filter(p => p.kind === "Announcement");
  const tickets = ["Student", "Admin", "Maintenance"].includes(state.user.role) ? await api("/api/maintenance").catch(() => []) : [];
  const pending = tickets.filter(t => t.status !== "Resolved" && t.status !== "Cancelled").length;
  const first = dash.fullName.split(" ")[0];
  const actions = [
    ["community", "New Post"], ["events", "Events"], ["market", "Marketplace"],
    ["groups", "Study Groups"], ["maintenance", "Maintenance"], ["rewards", "Rewards"]
  ].filter(a => (navByRole[state.user.role] || []).some(n => n[0] === a[0]));
  const canSeeFeed = ["Student", "Admin"].includes(state.user.role);

  main.innerHTML = `
    <div class="date-line">${new Date().toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric", year: "numeric" })}</div>
    <div class="page-head"><div><h1>${greeting()}, ${esc(first)}</h1><p>Here's what's happening at the residence today.</p></div></div>
    <div class="banner">Welcome to ResLink! We're excited to launch your new digital residence hub. Explore events, connect with neighbours, and report issues all in one place.</div>
    ${announcements.length ? `<div class="section-head"><h2>Announcements</h2></div>
    ${announcements.map(p => `<div class="card mb"><div class="between"><b>${esc(p.title)}</b><span class="muted">${fmtDate(p.createdAt)}</span></div><p>${esc(p.body)}</p></div>`).join("")}` : ""}
    <div class="stats">
      <div class="stat-card"><span class="muted">Points</span><b>${dash.points}</b></div>
      <div class="stat-card"><span class="muted">Room</span><b>${esc(state.user.room || "—")}</b></div>
      <div class="stat-card"><span class="muted">Maintenance</span><b>${pending} Pending</b></div>
    </div>
    ${actions.length ? `<div class="section-head"><h2>Quick Actions</h2></div>
    <div class="actions">${actions.map(a => `<button class="action" data-go="${a[0]}"><div class="ico">${icon(a[0])}</div>${a[1]}</button>`).join("")}</div>` : ""}
    ${events.length ? `<div class="section-head"><h2>Upcoming Events</h2><button class="link" data-go="events">View all</button></div>
    <div class="grid-3 mb">${events.slice(0, 3).map(eventCard).join("")}</div>` : ""}
    ${canSeeFeed && posts.length ? `<div class="section-head"><h2>Recent Posts</h2><button class="link" data-go="community">View all</button></div>
    <div class="card">${posts.filter(p => p.kind !== "Announcement").slice(0, 3).map(p => `<div class="feed-item"><div class="avatar">${initial(p.authorName)}</div><div><b>${esc(p.authorName)}</b> <span class="tag ${tagClass(p.authorRole)}">${esc(p.authorRole)}</span><p>${esc(p.body)}</p></div></div>`).join("")}</div>` : ""}
    ${dash.analytics && state.user.role !== "Student" ? `<div class="section-head"><h2>Today's pulse</h2></div>
    <div class="stats"><div class="stat-card"><span class="muted">Open tickets</span><b>${dash.analytics.openTickets}</b></div>
    <div class="stat-card"><span class="muted">Complaints</span><b>${dash.analytics.openComplaints}</b></div>
    <div class="stat-card"><span class="muted">Panic alerts</span><b>${dash.analytics.openEmergencies}</b></div></div>` : ""}`;
  main.querySelectorAll("[data-go]").forEach(b => b.onclick = () => { state.page = b.dataset.go; render(); });
}

// Seeded events use saved stock photos so the cards do not depend on another host (Unsplash, 2026).
function eventPhoto(e) {
  const title = (e.title || "").toLowerCase();
  if (title.includes("movie")) return "events/movie.jpg";
  if (title.includes("braai")) return "events/braai.jpg";
  if (title.includes("football")) return "events/football.jpg";
  if (title.includes("study") || title.includes("exam")) return "events/study.jpg";
  if (title.includes("yoga")) return "events/yoga.jpg";
  if (title.includes("heritage")) return "events/heritage.jpg";
  if (title.includes("meeting")) return "events/meeting.jpg";
  const byCategory = { Social: "events/braai.jpg", Sports: "events/football.jpg", Academic: "events/study.jpg", Cultural: "events/heritage.jpg", Meeting: "events/meeting.jpg" };
  return byCategory[e.category] || "events/meeting.jpg";
}

function eventCard(e) {
  return `<div class="card" style="padding:0;overflow:hidden">
    <img class="event-photo" src="${eventPhoto(e)}" alt="" />
    <div class="event-body">
      <span class="tag ${tagClass(e.category)}">${esc(e.category)}</span>
      ${e.isFeatured ? ` <span class="tag yellow">Featured</span>` : ""}
      <h3 style="margin:10px 0 6px">${esc(e.title)}</h3>
      <p class="muted">${fmtDate(e.startsAt)}<br>${esc(e.location)} · ${e.rsvpCount} attending</p>
      ${state.user.role === "Student" ? `<p><button class="btn rsvp" data-id="${e.id}" data-on="${e.hasRsvp}">${e.hasRsvp ? "Cancel RSVP" : "RSVP"}</button></p>` : ""}
    </div></div>`;
}

async function community(main) {
  const posts = await api("/api/posts");
  const filtered = state.filter === "All" ? posts : posts.filter(p => p.kind === state.filter);
  const canPost = ["Student", "Admin"].includes(state.user.role);
  main.innerHTML = `
    <div class="page-head"><div><h1>Community Feed</h1><p>Share updates and connect with residents.</p></div></div>
    ${canPost ? `<div class="card mb">
      <div class="composer-tabs">
        <button class="pill ${state.tab === "Post" || state.tab === "overview" ? "active" : ""}" data-kind="Post">Post</button>
        <button class="pill" data-kind="Discussion">Discussion</button>
        <button class="pill" data-kind="Poll">Poll</button>
      </div>
      <input id="p-title" placeholder="Title (optional)" />
      <textarea id="p-body" placeholder="What's on your mind, ${esc(state.user.fullName.split(" ")[0])}?"></textarea>
      <div id="poll-opts" class="hidden"><label>Poll options<input id="p-opts" value="Braai, Movie night" /></label></div>
      <p class="row" style="justify-content:flex-end"><button class="btn" id="share">Share</button></p>
    </div>` : ""}
    <div class="pills">${["All", "Post", "Discussion", "Poll", "Announcement"].map(f => `<button class="pill ${state.filter === f ? "active" : ""}" data-filter="${f}">${f}</button>`).join("")}</div>
    ${filtered.map(p => `<div class="card mb">
      <div class="feed-item"><div class="avatar">${initial(p.authorName)}</div><div style="flex:1">
        <b>${esc(p.authorName)}</b> <span class="tag ${tagClass(p.authorRole)}">${esc(p.authorRole)}</span>
        <span class="tag gray">${esc(p.kind)}</span>
        <div class="muted">${fmtDate(p.createdAt)}</div>
        ${p.title ? `<h3 style="margin:8px 0 4px">${esc(p.title)}</h3>` : ""}
        <p>${esc(p.body)}</p>
        ${(p.pollOptions || []).map(o => `<div class="muted poll" data-id="${p.id}" data-i="${o.index}" style="cursor:pointer;margin:6px 0">${esc(o.label)} · ${o.votes}</div>`).join("")}
        <div class="muted">♥ ${p.likeCount} · ${p.comments.length} comments</div>
        ${p.comments.map(c => `<p class="muted">${esc(c.authorName)}: ${esc(c.body)}</p>`).join("")}
        ${canPost ? `<div class="row"><input id="c-${p.id}" placeholder="Write a comment" /><button class="btn ghost comment" data-id="${p.id}">Reply</button></div>` : ""}
        ${state.user.role === "Admin" || p.authorId === state.user.userId ? `<button class="btn ghost del" data-id="${p.id}">Remove</button>` : ""}
      </div></div></div>`).join("")}`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; community(main); });
  main.querySelectorAll("[data-kind]").forEach(b => b.onclick = () => {
    state.tab = b.dataset.kind;
    main.querySelectorAll("[data-kind]").forEach(x => x.classList.toggle("active", x === b));
    document.getElementById("poll-opts").classList.toggle("hidden", state.tab !== "Poll");
  });
  const share = document.getElementById("share");
  if (share) share.onclick = async () => {
    await api("/api/posts", { method: "POST", body: JSON.stringify({ title: val("p-title") || "Update", body: val("p-body"), kind: state.tab === "overview" ? "Post" : state.tab, isPoll: state.tab === "Poll", pollOptions: val("p-opts").split(",").map(s => s.trim()) }) });
    community(main);
  };
  main.querySelectorAll(".poll").forEach(el => el.onclick = async () => { await api(`/api/posts/${el.dataset.id}/vote`, { method: "POST", body: JSON.stringify({ optionIndex: Number(el.dataset.i) }) }); community(main); });
  main.querySelectorAll(".comment").forEach(el => el.onclick = async () => { await api(`/api/posts/${el.dataset.id}/comments`, { method: "POST", body: JSON.stringify({ body: val(`c-${el.dataset.id}`) }) }); community(main); });
  main.querySelectorAll(".del").forEach(el => el.onclick = async () => { await api(`/api/posts/${el.dataset.id}`, { method: "DELETE" }); community(main); });
}

async function events(main) {
  const items = await api("/api/events");
  const filtered = state.filter === "All" ? items : items.filter(e => e.category === state.filter);
  main.innerHTML = `
    <div class="page-head"><div><h1>Events</h1><p>Discover and join residence events.</p></div>
      ${state.user.role === "Admin" ? `<button class="btn" id="toggle-create">+ Create Event</button>` : ""}</div>
    <div class="card hidden mb" id="create-box">
      <label>Title<input id="e-title" /></label><label>Description<input id="e-desc" /></label>
      <label>Location<input id="e-loc" /></label><label>Starts at<input id="e-when" value="2026-08-20T17:00:00Z" /></label>
      <label>Category<select id="e-cat"><option>Social</option><option>Academic</option><option>Sports</option><option>Cultural</option><option>Meeting</option><option>Other</option></select></label>
      <button class="btn" id="create-event">Publish event</button>
    </div>
    <div class="pills">${["All", "Social", "Academic", "Sports", "Cultural", "Meeting", "Other"].map(f => `<button class="pill ${state.filter === f ? "active" : ""}" data-filter="${f}">${f}</button>`).join("")}</div>
    <div class="grid-3">${filtered.map(eventCard).join("")}</div>`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; events(main); });
  const toggle = document.getElementById("toggle-create");
  if (toggle) toggle.onclick = () => document.getElementById("create-box").classList.toggle("hidden");
  const create = document.getElementById("create-event");
  if (create) create.onclick = async () => {
    await api("/api/events", { method: "POST", body: JSON.stringify({ title: val("e-title"), description: val("e-desc"), location: val("e-loc"), startsAt: val("e-when"), category: val("e-cat") }) });
    events(main);
  };
  main.querySelectorAll(".rsvp").forEach(el => el.onclick = async () => {
    await api(`/api/events/${el.dataset.id}/rsvp`, { method: el.dataset.on === "true" ? "DELETE" : "POST" });
    events(main);
  });
}

// Marketplace, groups, tickets, and rewards expose undo so a lecturer tap is not final.
async function market(main) {
  const items = await api("/api/marketplace");
  const filtered = state.filter === "All" ? items : items.filter(i => i.category === state.filter);
  const sell = state.user.role === "Student";
  main.innerHTML = `
    <div class="page-head"><div><h1>Marketplace</h1><p>Buy, sell and trade with fellow residents.</p></div>
      ${sell ? `<button class="btn" id="toggle-sell">+ Sell Item</button>` : ""}</div>
    <div class="card hidden mb" id="sell-box">
      <label>Item<input id="m-title" /></label><label>Description<input id="m-desc" /></label>
      <label>Category<select id="m-cat"><option>Textbooks</option><option>Electronics</option><option>Furniture</option><option>Clothing</option><option>Food</option><option>Services</option><option>Other</option></select></label>
      <label>Condition<select id="m-cond"><option>Like New</option><option>Good</option><option>Fair</option></select></label>
      <label>Price (R)<input id="m-price" type="number" value="150" /></label>
      <button class="btn" id="list-item">List item</button>
    </div>
    <div class="pills">${["All", "Textbooks", "Electronics", "Furniture", "Clothing", "Food", "Services", "Other"].map(f => `<button class="pill ${state.filter === f ? "active" : ""}" data-filter="${f}">${f}</button>`).join("")}</div>
    <div class="grid-3">${filtered.map(i => `<div class="card" style="padding:0;overflow:hidden">
      <div class="event-art">${icon("market")}</div>
      <div class="event-body">
        <div class="between"><h3 style="margin:0">${esc(i.title)}</h3><span class="price">${i.price === 0 ? "FREE" : "R" + i.price}</span></div>
        <p class="muted">${esc(i.description)}</p>
        <p><span class="tag ${tagClass(i.condition)}">${esc(i.condition)}</span> <span class="tag gray">${esc(i.category)}</span></p>
        <p class="muted">${esc(i.sellerName)}${i.sellerRoom ? " · Room " + esc(i.sellerRoom) : ""}</p>
        ${sell && !i.isSold ? `<button class="btn ghost sold" data-id="${i.id}">Mark sold</button>` : ""}
        ${sell && i.isSold ? `<button class="btn ghost unsold" data-id="${i.id}">Mark available</button>` : ""}
      </div></div>`).join("")}</div>`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; market(main); });
  const t = document.getElementById("toggle-sell"); if (t) t.onclick = () => document.getElementById("sell-box").classList.toggle("hidden");
  const l = document.getElementById("list-item"); if (l) l.onclick = async () => {
    await api("/api/marketplace", { method: "POST", body: JSON.stringify({ title: val("m-title"), description: val("m-desc"), category: val("m-cat"), condition: val("m-cond"), price: Number(val("m-price")) }) });
    market(main);
  };
  main.querySelectorAll(".sold").forEach(el => el.onclick = async () => { await api(`/api/marketplace/${el.dataset.id}/sold`, { method: "PUT" }); market(main); });
  main.querySelectorAll(".unsold").forEach(el => el.onclick = async () => { await api(`/api/marketplace/${el.dataset.id}/unsold`, { method: "PUT" }); market(main); });
}

async function groups(main) {
  const items = await api("/api/study-groups");
  const mine = state.filter === "Mine" ? items.filter(g => g.isMember) : items;
  main.innerHTML = `
    <div class="page-head"><div><h1>Study Groups</h1><p>Collaborate with fellow residents on your courses.</p></div>
      ${state.user.role === "Student" || state.user.role === "Admin" ? `<button class="btn" id="toggle-g">+ Create Group</button>` : ""}</div>
    <div class="card hidden mb" id="g-box">
      <label>Name<input id="g-name" /></label><label>Topic<input id="g-topic" /></label><label>Course code<input id="g-code" /></label>
      <label>Description<input id="g-desc" /></label><label>Schedule<input id="g-sch" /></label><label>Location<input id="g-loc" /></label>
      <button class="btn" id="g-create">Create group</button>
    </div>
    <div class="pills">
      <button class="pill ${state.filter !== "Mine" ? "active" : ""}" data-filter="All">All Groups</button>
      <button class="pill ${state.filter === "Mine" ? "active" : ""}" data-filter="Mine">My Groups (${items.filter(g => g.isMember).length})</button>
    </div>
    <div class="grid-3">${mine.map(g => `<div class="card">
      <div class="icon-box">${icon("groups")}</div>
      <div class="muted">${esc(g.topic)} · ${esc(g.courseCode)}</div>
      <h3>${esc(g.name)}</h3>
      <p class="muted">${esc(g.description)}</p>
      <p class="muted">${esc(g.schedule)}<br>${esc(g.location)}<br>${g.memberCount}/${g.maxMembers} members</p>
      ${g.isMember ? `<div class="row"><span class="tag green">Member</span><button class="btn ghost leave" data-id="${g.id}">Leave</button></div>` : `<button class="btn join" data-id="${g.id}">Join</button>`}
    </div>`).join("")}</div>`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; groups(main); });
  const tg = document.getElementById("toggle-g"); if (tg) tg.onclick = () => document.getElementById("g-box").classList.toggle("hidden");
  const c = document.getElementById("g-create"); if (c) c.onclick = async () => {
    await api("/api/study-groups", { method: "POST", body: JSON.stringify({ name: val("g-name"), topic: val("g-topic"), description: val("g-desc"), courseCode: val("g-code"), schedule: val("g-sch"), location: val("g-loc") }) });
    groups(main);
  };
  main.querySelectorAll(".join").forEach(el => el.onclick = async () => { await api(`/api/study-groups/${el.dataset.id}/join`, { method: "POST" }); groups(main); });
  main.querySelectorAll(".leave").forEach(el => el.onclick = async () => { await api(`/api/study-groups/${el.dataset.id}/leave`, { method: "DELETE" }); groups(main); });
}

async function maintenance(main) {
  const tickets = await api("/api/maintenance");
  const map = { All: () => true, Pending: t => t.status === "Open", "In Progress": t => t.status === "InProgress", Resolved: t => t.status === "Resolved", Cancelled: t => t.status === "Cancelled" };
  const filtered = tickets.filter(map[state.filter] || map.All);
  const student = state.user.role === "Student";
  const staff = ["Maintenance", "Admin"].includes(state.user.role);
  main.innerHTML = `
    <div class="page-head"><div><h1>Maintenance</h1><p>${staff ? "Manage maintenance requests" : "Report and track residence issues"}.</p></div>
      ${student ? `<button class="btn" id="toggle-t">+ New request</button>` : ""}</div>
    <div class="card hidden mb" id="t-box">
      <label>Issue<input id="t-title" /></label><label>Details<textarea id="t-desc"></textarea></label>
      <label>Location<input id="t-loc" /></label>
      <label>Category<select id="t-cat"><option>Plumbing</option><option>Electrical</option><option>Furniture</option><option>General</option></select></label>
      <label>Priority<select id="t-pri"><option>Medium</option><option>High</option><option>Low</option></select></label>
      <button class="btn" id="new-ticket">Submit</button>
    </div>
    <div class="pills">${["All", "Pending", "In Progress", "Resolved", "Cancelled"].map(f => `<button class="pill ${state.filter === f ? "active" : ""}" data-filter="${f}">${f}</button>`).join("")}</div>
    ${filtered.map(t => `<div class="card mb">
      <div class="between"><h3 style="margin:0">${esc(t.title)}</h3><div><span class="tag ${tagClass(prettyStatus(t.status))}">${prettyStatus(t.status)}</span> <span class="tag ${tagClass(t.priority)}">${esc(t.priority)}</span> <span class="muted">${esc(t.category)}</span></div></div>
      <p class="muted">${esc(t.reporterName)} · ${esc(t.location)} · ${fmtDate(t.createdAt)}</p>
      <p>${esc(t.description)}</p>
      ${t.resolutionNotes ? `<p class="muted">Notes: ${esc(t.resolutionNotes)}</p>` : ""}
      ${staff ? `<div class="row">
        ${t.status !== "Open" ? `<button class="btn ghost st" data-id="${t.id}" data-s="Open">Reopen</button>` : ""}
        ${t.status === "Open" ? `<button class="btn blue st" data-id="${t.id}" data-s="InProgress">Start Work</button>` : ""}
        ${t.status !== "Resolved" ? `<button class="btn ghost st" data-id="${t.id}" data-s="Resolved">Mark Resolved</button>` : ""}
        <button class="btn ghost st" data-id="${t.id}" data-s="Cancelled">Cancel</button>
      </div>` : ""}
      ${student && t.status === "Open" ? `<button class="btn ghost cancel-t" data-id="${t.id}">Cancel request</button>` : ""}
    </div>`).join("")}`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; maintenance(main); });
  const tt = document.getElementById("toggle-t"); if (tt) tt.onclick = () => document.getElementById("t-box").classList.toggle("hidden");
  const nt = document.getElementById("new-ticket"); if (nt) nt.onclick = async () => {
    await api("/api/maintenance", { method: "POST", body: JSON.stringify({ title: val("t-title"), description: val("t-desc"), location: val("t-loc"), category: val("t-cat"), priority: val("t-pri") }) });
    maintenance(main);
  };
  main.querySelectorAll(".st").forEach(el => el.onclick = async () => {
    await api(`/api/maintenance/${el.dataset.id}/status`, { method: "PUT", body: JSON.stringify({ status: el.dataset.s, resolutionNotes: "Updated by staff" }) });
    maintenance(main);
  });
  main.querySelectorAll(".cancel-t").forEach(el => el.onclick = async () => {
    await api(`/api/maintenance/${el.dataset.id}/status`, { method: "PUT", body: JSON.stringify({ status: "Cancelled", resolutionNotes: "Cancelled by student" }) });
    maintenance(main);
  });
}

async function security(main) {
  if (state.user.role === "Student") {
    const mineAlerts = await api("/api/emergencies/mine").catch(() => []);
    const mineNoise = await api("/api/noise/mine").catch(() => []);
    main.innerHTML = `
      <div class="page-head"><div><h1>Security</h1><p>Report a visitor, noise issue, or emergency.</p></div></div>
      <div class="grid-2">
        <div class="card"><h3>Panic button</h3><p class="muted">Your room ${esc(state.user.room || "")} will be sent to security.</p><button class="btn danger" id="panic">Send panic alert</button><p id="p-msg"></p></div>
        <div class="card"><h3>Noise complaint</h3>
          <label>Location<input id="n-loc" /></label><label>What is happening?<input id="n-desc" /></label>
          <label><input id="n-anon" type="checkbox" checked /> Report anonymously</label>
          <button class="btn" id="n-send">Submit complaint</button><p id="n-msg"></p></div>
        <div class="card"><h3>Register a visitor</h3>
          <label>Visitor name<input id="v-name" /></label><label>Your room<input id="v-room" value="${esc(state.user.room || "")}" /></label>
          <label>Purpose<input id="v-purpose" /></label><button class="btn" id="v-send">Register visitor</button><p id="v-msg"></p></div>
      </div>
      ${mineAlerts.length ? `<div class="section-head"><h2>Your panic alerts</h2></div>
        ${mineAlerts.map(a => `<div class="card mb between"><div><b>${esc(a.status)}</b><div class="muted">${esc(a.location)} · ${fmtDate(a.createdAt)}</div></div>
        ${a.status === "Open" ? `<button class="btn ghost wd-em" data-id="${a.id}">False alarm</button>` : ""}</div>`).join("")}` : ""}
      ${mineNoise.length ? `<div class="section-head"><h2>Your noise reports</h2></div>
        ${mineNoise.map(n => `<div class="card mb between"><div><b>${esc(n.location)}</b><div class="muted">${esc(n.status)} · ${fmtDate(n.createdAt)}</div></div>
        ${n.status === "Open" ? `<button class="btn ghost wd-ns" data-id="${n.id}">Withdraw</button>` : ""}</div>`).join("")}` : ""}`;
    document.getElementById("panic").onclick = async () => { await api("/api/emergencies", { method: "POST", body: JSON.stringify({ message: "Panic button activated" }) }); security(main); };
    document.getElementById("n-send").onclick = async () => { await api("/api/noise", { method: "POST", body: JSON.stringify({ location: val("n-loc"), description: val("n-desc"), isAnonymous: document.getElementById("n-anon").checked }) }); security(main); };
    document.getElementById("v-send").onclick = async () => { await api("/api/visitors", { method: "POST", body: JSON.stringify({ visitorName: val("v-name"), hostRoom: val("v-room"), purpose: val("v-purpose") }) }); document.getElementById("v-msg").textContent = "Visitor registered."; };
    main.querySelectorAll(".wd-em").forEach(el => el.onclick = async () => { await api(`/api/emergencies/${el.dataset.id}/withdraw`, { method: "PUT" }); security(main); });
    main.querySelectorAll(".wd-ns").forEach(el => el.onclick = async () => { await api(`/api/noise/${el.dataset.id}/withdraw`, { method: "PUT" }); security(main); });
    return;
  }
  const visitors = await api("/api/visitors").catch(() => []);
  const alerts = await api("/api/emergencies");
  const noise = await api("/api/noise");
  const tab = state.tab === "incidents" ? "incidents" : "visitors";
  main.innerHTML = `
    <div class="page-head"><div><h1>Security</h1><p>Manage visitors, incidents and access logs.</p></div>
      <button class="btn" id="toggle-v">+ Register Visitor</button></div>
    <div class="card hidden mb" id="v-box">
      <label>Visitor name<input id="v-name" /></label><label>Host room<input id="v-room" /></label><label>Purpose<input id="v-purpose" /></label>
      <button class="btn" id="v-send">Register</button>
    </div>
    <div class="pills">
      <button class="pill ${tab === "visitors" ? "active" : ""}" data-tab="visitors">Visitors</button>
      <button class="pill ${tab === "incidents" ? "active" : ""}" data-tab="incidents">Incidents</button>
    </div>
    ${tab === "visitors" ? (visitors.length ? visitors.map(v => `<div class="card mb between"><div><b>${esc(v.visitorName)}</b><div class="muted">Host ${esc(v.hostName)} · ${esc(v.hostRoom)} · ${esc(v.purpose)}</div></div>
        <div>${v.status === "OnSite" ? `<button class="btn ghost v-out" data-id="${v.id}">Check out</button>` : `<span class="tag gray">${esc(v.status)}</span>`}</div></div>`).join("") : `<div class="empty">No visitors logged today</div>`)
      : `${alerts.map(a => `<div class="card mb"><div class="between"><b>${esc(a.reporterName)}</b><span class="tag ${tagClass(prettyStatus(a.status))}">${prettyStatus(a.status)}</span></div><p class="muted">${esc(a.location)}</p><p>${esc(a.message)}</p>
        ${a.status !== "Open" ? `<button class="btn ghost em" data-id="${a.id}" data-s="Open">Reopen</button>` : ""}
        <button class="btn ghost em" data-id="${a.id}" data-s="Acknowledged">Acknowledge</button>
        <button class="btn ghost em" data-id="${a.id}" data-s="Resolved">Resolve</button>
        <button class="btn ghost em" data-id="${a.id}" data-s="Withdrawn">False alarm</button></div>`).join("")}
        ${noise.map(n => `<div class="card mb"><div class="between"><b>${esc(n.location)}</b><span class="tag ${tagClass(prettyStatus(n.status))}">${prettyStatus(n.status)}</span></div>
        <p class="muted">${esc(n.reporterName)}</p><p>${esc(n.description)}</p>
        ${n.status !== "Open" ? `<button class="btn ghost ns" data-id="${n.id}" data-s="Open">Reopen</button>` : ""}
        <button class="btn ghost ns" data-id="${n.id}" data-s="Resolved">Mark resolved</button>
        <button class="btn ghost ns" data-id="${n.id}" data-s="Withdrawn">Withdraw</button></div>`).join("")}`}`;
  main.querySelectorAll("[data-tab]").forEach(b => b.onclick = () => { state.tab = b.dataset.tab; security(main); });
  document.getElementById("toggle-v").onclick = () => document.getElementById("v-box").classList.toggle("hidden");
  document.getElementById("v-send").onclick = async () => { await api("/api/visitors", { method: "POST", body: JSON.stringify({ visitorName: val("v-name"), hostRoom: val("v-room"), purpose: val("v-purpose") }) }); security(main); };
  main.querySelectorAll(".em").forEach(el => el.onclick = async () => { await api(`/api/emergencies/${el.dataset.id}/status`, { method: "PUT", body: JSON.stringify({ status: el.dataset.s }) }); security(main); });
  main.querySelectorAll(".ns").forEach(el => el.onclick = async () => { await api(`/api/noise/${el.dataset.id}/status`, { method: "PUT", body: JSON.stringify({ status: el.dataset.s }) }); security(main); });
  main.querySelectorAll(".v-out").forEach(el => el.onclick = async () => { await api(`/api/visitors/${el.dataset.id}/status`, { method: "PUT", body: JSON.stringify({ status: "Departed" }) }); security(main); });
}

async function rewards(main) {
  const items = await api("/api/rewards");
  const redemptions = state.user.role === "Student" ? await api("/api/rewards/redemptions").catch(() => []) : [];
  const filtered = state.filter === "All" ? items : items.filter(r => r.category === state.filter);
  main.innerHTML = `
    <div class="balance">
      <div><small>Your reward balance</small><h1 style="margin:8px 0">${state.user.points} points</h1><p>Earn points by attending events, posting and helping others.</p></div>
      <div class="row"><div class="metric"><b>${items.filter(r => state.user.points >= r.pointsCost).length}</b><div>Redeemable</div></div>
      <div class="metric"><b>${items.length}</b><div>Total Rewards</div></div></div>
    </div>
    <div class="section-head"><h2>Reward Catalog</h2>${state.user.role === "Admin" ? `<button class="btn" id="toggle-r">+ Add Reward</button>` : ""}</div>
    <div class="card hidden mb" id="r-box">
      <label>Name<input id="r-name" /></label><label>Description<input id="r-desc" /></label>
      <label>Category<select id="r-cat"><option>Food</option><option>Merchandise</option><option>Services</option><option>Experiences</option><option>Discounts</option></select></label>
      <label>Points<input id="r-cost" type="number" value="50" /></label>
      <button class="btn" id="r-add">Add</button>
    </div>
    <div class="pills">${["All", "Food", "Merchandise", "Services", "Experiences", "Discounts"].map(f => `<button class="pill ${state.filter === f ? "active" : ""}" data-filter="${f}">${f}</button>`).join("")}</div>
    <div class="grid-3">${filtered.map(r => {
      const need = r.pointsCost - state.user.points;
      return `<div class="card"><div class="icon-box">${icon("rewards")}</div><h3>${esc(r.name)}</h3><p class="muted">${esc(r.description)}</p>
        <p><span class="tag green">${r.pointsCost} pts</span> <span class="muted">${r.stock} left</span></p>
        ${state.user.role === "Student" ? (need > 0 ? `<button class="btn wide" disabled>Need ${need} more pts</button>` : `<button class="btn wide redeem" data-id="${r.id}" data-cost="${r.pointsCost}">Redeem</button>`) : ""}
      </div>`;
    }).join("")}</div>
    ${redemptions.length ? `<div class="section-head"><h2>Recent redemptions</h2></div>
      ${redemptions.map(r => `<div class="card mb between"><div><b>${esc(r.rewardName)}</b><div class="muted">${r.pointsCost} pts · ${fmtDate(r.createdAt)}</div></div>
      <button class="btn ghost undo-r" data-id="${r.id}" data-cost="${r.pointsCost}">Undo redeem</button></div>`).join("")}` : ""}`;
  main.querySelectorAll("[data-filter]").forEach(b => b.onclick = () => { state.filter = b.dataset.filter; rewards(main); });
  const tr = document.getElementById("toggle-r"); if (tr) tr.onclick = () => document.getElementById("r-box").classList.toggle("hidden");
  const ra = document.getElementById("r-add"); if (ra) ra.onclick = async () => {
    await api("/api/rewards", { method: "POST", body: JSON.stringify({ name: val("r-name"), description: val("r-desc"), category: val("r-cat"), pointsCost: Number(val("r-cost")) }) });
    rewards(main);
  };
  main.querySelectorAll(".redeem").forEach(el => el.onclick = async () => {
    await api(`/api/rewards/${el.dataset.id}/redeem`, { method: "POST" });
    state.user.points -= Number(el.dataset.cost); save(state.user); rewards(main);
  });
  main.querySelectorAll(".undo-r").forEach(el => el.onclick = async () => {
    await api(`/api/rewards/redemptions/${el.dataset.id}/cancel`, { method: "POST" });
    state.user.points += Number(el.dataset.cost); save(state.user); rewards(main);
  });
}

async function admin(main) {
  const users = await api("/api/admin/users");
  const analytics = await api("/api/admin/analytics");
  const posts = await api("/api/posts");
  const events = await api("/api/events");
  const market = await api("/api/marketplace");
  const rewards = await api("/api/rewards");
  const tab = state.tab === "residents" || state.tab === "announcements" ? state.tab : "overview";
  main.innerHTML = `
    <div class="page-head"><div><h1>Admin Panel</h1><p>Manage your residence community.</p></div></div>
    <div class="pills">
      <button class="pill ${tab === "overview" ? "active" : ""}" data-tab="overview">Overview</button>
      <button class="pill ${tab === "residents" ? "active" : ""}" data-tab="residents">Residents</button>
      <button class="pill ${tab === "announcements" ? "active" : ""}" data-tab="announcements">Announcements</button>
    </div>
    ${tab === "overview" ? `<div class="admin-metrics">
      ${metric("Total Residents", analytics.students, "groups")}
      ${metric("Maintenance Requests", analytics.openTickets, "maintenance")}
      ${metric("Events Created", events.length, "events")}
      ${metric("Community Posts", posts.length, "community")}
      ${metric("Marketplace Listings", market.length, "market")}
      ${metric("Active Rewards", rewards.length, "rewards")}
    </div>` : tab === "residents" ? users.map(u => `<div class="card mb between"><div><b>${esc(u.fullName)}</b><div class="muted">${esc(u.email)}${u.room ? " · Room " + esc(u.room) : ""}</div></div>
      <div><span class="tag ${tagClass(u.role)}">${esc(u.isActive ? u.role : "Inactive")}</span>
      <button class="btn ghost act" data-id="${u.id}" data-on="${u.isActive}">${u.isActive ? "Deactivate" : "Activate"}</button></div></div>`).join("")
      : `<div class="card mb"><textarea id="a-body" placeholder="Write an announcement"></textarea><p><button class="btn" id="a-post">Post announcement</button></p></div>
        ${posts.filter(p => p.kind === "Announcement").map(p => `<div class="card mb"><b>${esc(p.title)}</b><p>${esc(p.body)}</p></div>`).join("")}`}`;
  main.querySelectorAll("[data-tab]").forEach(b => b.onclick = () => { state.tab = b.dataset.tab; admin(main); });
  main.querySelectorAll(".act").forEach(el => el.onclick = async () => { await api(`/api/admin/users/${el.dataset.id}/active`, { method: "PUT", body: JSON.stringify({ isActive: el.dataset.on !== "true" }) }); admin(main); });
  const ap = document.getElementById("a-post"); if (ap) ap.onclick = async () => {
    await api("/api/posts", { method: "POST", body: JSON.stringify({ title: "Announcement", body: val("a-body"), kind: "Announcement", isPoll: false }) });
    admin(main);
  };
}
const metric = (label, n, name) => `<div class="card"><div class="icon-box">${icon(name)}</div><b>${n}</b><div class="muted">${label}</div></div>`;

async function notifications(main) {
  const items = await api("/api/notifications");
  main.innerHTML = `<div class="page-head"><div><h1>Notifications</h1><p>Residence alerts and ticket updates.</p></div></div>
    ${items.map(n => `<div class="card mb"><div class="between"><b>${esc(n.title)}</b>${n.isRead ? "" : `<span class="tag green">New</span>`}</div><p class="muted">${esc(n.body)}</p></div>`).join("") || `<div class="empty">No notifications yet</div>`}`;
}

function profile(main) {
  const initials = state.user.fullName.split(" ").map(p => p[0]).join("").slice(0, 2).toUpperCase();
  main.innerHTML = `
    <div class="page-head"><div><h1>My Profile</h1><p>Manage your account and personal details.</p></div></div>
    <div class="card mb">
      <div class="profile-hero">
        <div class="big">${initials}</div>
        <div class="who"><h2 style="margin:0">${esc(state.user.fullName)}</h2><small>${esc(state.user.role)}</small><p class="muted">${esc(state.user.email)}</p></div>
      </div>
      <div class="card" style="background:#f7f8f6;margin-top:20px"><b>${state.user.points} points</b><div class="muted">Reward balance</div></div>
    </div>
    <button class="card signout-card" id="logout2">Sign out of ResLink</button>`;
  document.getElementById("logout2").onclick = () => { save(null); render(); };
}

render();
