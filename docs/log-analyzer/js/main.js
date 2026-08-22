async function pasteResult() {
  try {
    const text = await navigator.clipboard.readText();
    if (!text) return alert("Clipboard is empty");
    document.getElementById("resultInput").value = text;
    analyzeResults();
  } catch (err) {
    alert("No access to clipboard: " + err);
  }
}

function safe(value) {
  return String(value ?? "").replace(/[&<>"']/g, (char) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;"
  })[char]);
}

function parseResults(text) {
  const lines = text.split(/\r?\n/).filter((line) => line.length);
  if (lines[0] !== "CRAPFIXER_RESULTS_V1")
    throw new Error("This is not a CrapFixer V1 result export.");

  const data = { meta: {}, summary: null, results: [], registry: [] };
  for (const line of lines.slice(1)) {
    const parts = line.split("\t");
    if (parts[0] === "META" && parts.length >= 3)
      data.meta[parts[1]] = parts.slice(2).join(" ");
    else if (parts[0] === "SUMMARY" && parts.length >= 4)
      data.summary = { total: Number(parts[1]), issues: Number(parts[2]), ok: Number(parts[3]) };
    else if (parts[0] === "RESULT" && parts.length >= 5)
      data.results.push({ kind: parts[1], name: parts[2], status: parts[3], current: parts.slice(4).join(" ") });
    else if (parts[0] === "REGISTRY" && parts.length >= 4)
      data.registry.push({ tweak: parts[1], path: parts[2], recommended: parts.slice(3).join(" ") });
  }
  return data;
}

function resultRows(items, cssName) {
  if (!items.length) return "<p>None.</p>";
  return items.map((item) => `
    <div class="${cssName}"><strong>${safe(item.name)}</strong><br>
    Status: ${safe(item.status)} &nbsp; Current: <code>${safe(item.current)}</code></div>`).join("");
}

function analyzeResults() {
  const output = document.getElementById("output");
  try {
    const data = parseResults(document.getElementById("resultInput").value.trim());
    const issues = data.results.filter((item) => item.kind === "ISSUE");
    const healthy = data.results.filter((item) => item.kind === "OK");
    const summary = data.summary || { total: data.results.length, issues: issues.length, ok: healthy.length };

    output.innerHTML = `
      <h3>CrapFixer ${safe(data.meta.Version || "")} results</h3>
      <p>${summary.total} checked &nbsp; ${summary.issues} issues &nbsp; ${summary.ok} correctly configured</p>
      <h3>Issues</h3>
      ${resultRows(issues, "issue")}
      <hr>
      <h3>Correctly configured</h3>
      ${resultRows(healthy, "healthy")}
      <hr>
      <h3>Registry details</h3>
      ${data.registry.length ? data.registry.map((entry) => `
        <div class="key"><strong>${safe(entry.tweak)}</strong><br>${safe(entry.path)}<br>
        Recommended: <code>${safe(entry.recommended)}</code></div>`).join("") : "<p>None.</p>"}
    `;
  } catch (err) {
    output.innerHTML = `<div class="issue"><strong>Could not read the results.</strong><br>${safe(err.message)}</div>`;
  }
}

function captureResult() {
  html2canvas(document.getElementById("output")).then((canvas) => {
    const link = document.createElement("a");
    link.download = "CrapFixer-results.png";
    link.href = canvas.toDataURL();
    link.click();
  });
}

function shareResult() {
  const text = document.getElementById("output").innerText;
  if (navigator.share) navigator.share({ title: "CrapFixer Analysis Results", text }).catch(() => {});
  else alert("Sharing is not supported by your browser.");
}

function shareOnTwitter() {
  const output = document.getElementById("output");
  if (!output.innerText.trim()) return alert("No results to share yet.");
  html2canvas(output).then((canvas) => {
    const win = window.open();
    win.document.write("<h2>Screenshot ready for X</h2>");
    win.document.write("<p>Save the image and attach it to your post.</p>");
    win.document.write(`<img src="${canvas.toDataURL("image/png")}" style="max-width:100%;border:1px solid #ccc;" />`);
    win.document.write('<p><a href="https://twitter.com/intent/tweet?text=Check%20out%20my%20CrapFixer%20analysis!&hashtags=CrapFixer" target="_blank">Post on X</a></p>');
  });
}
