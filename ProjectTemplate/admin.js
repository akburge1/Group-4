async function fetchWeeklyPrompt() {
    try {
        const res = await fetch("/ProjectServices.asmx/GetCurrentPrompt", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
        });
        const data = await res.json();
        const prompt = data.d;

        const weeklyEl = document.getElementById("weekly-prompt");
        if (!weeklyEl) return;

        if (prompt) {
            weeklyEl.textContent = cleanPromptText(prompt.text || "");
            const weekEl = document.getElementById("week-timeframe");
            if (weekEl && prompt.weekStartIso) {
                weekEl.textContent = `Week starting ${prompt.weekStartIso}`;
            }
        } else {
            weeklyEl.textContent = "No prompt available.";
        }
    } catch {
        const weeklyEl = document.getElementById("weekly-prompt");
        if (weeklyEl) weeklyEl.textContent = "Failed to load prompt.";
    }
}

function cleanPromptText(s) {
    return String(s || "")
        .normalize("NFKD")
        .replace(/[^\x20-\x7E]/g, " ")
        .replace(/\s+/g, " ")
        .trim();
}

async function fetchPromptList() {
    try {
        const res = await fetch("/ProjectServices.asmx/GetPromptList", {
            method: "POST",
            headers: { "Content-Type": "application/json" }
        });
        const data = await res.json();
        const prompts = data.d || [];

        const sel = document.getElementById("prompt-select");
        if (!sel) return;

        sel.innerHTML = `<option value="0">All prompts</option>`;

        prompts.forEach(p => {
            const opt = document.createElement("option");
            opt.value = String(p.id);
            opt.textContent = `${p.id} - ${cleanPromptText(p.text)}`;
            sel.appendChild(opt);
        });
    } catch { }
}

async function fetchFeedback() {
    try {
        const res = await fetch("/ProjectServices.asmx/GetAllFeedback", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ page: 1, pageSize: 50 })
        });
        const data = await res.json();
        const feedback = data.d?.items || [];
        renderFeedback(feedback);
    } catch {
        renderFeedback([]);
    }
}

async function fetchFilteredFeedback(dateFrom, dateTo, promptId) {
    try {
        const res = await fetch("/ProjectServices.asmx/GetFilteredFeedback", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                dateFrom: dateFrom || "",
                dateTo: dateTo || "",
                promptId: promptId ? parseInt(promptId, 10) : 0,
                page: 1,
                pageSize: 50
            })
        });
        const data = await res.json();
        const feedback = data.d?.items || [];
        renderFeedback(feedback);
    } catch {
        renderFeedback([]);
    }
}

function renderFeedback(feedback) {
    const tbody = document.getElementById("feedback-list");
    if (!tbody) return;

    tbody.innerHTML = "";

    if (!feedback.length) {
        const row = document.createElement("tr");
        row.innerHTML = `<td colspan="4">No feedback found.</td>`;
        tbody.appendChild(row);
        return;
    }

    feedback.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${item.dateSubmitted}</td>
            <td>${item.displayName}</td>
            <td>${item.promptText}</td>
            <td>${item.message}</td>
        `;
        tbody.appendChild(row);
    });
}

function handleLogout() {
    fetch("/ProjectServices.asmx/Logout", {
        method: "POST",
        headers: { "Content-Type": "application/json" }
    })
        .then(res => res.json())
        .then(() => {
            window.location.href = "login.html";
        })
        .catch(err => {
            console.error("Logout failed:", err);
            window.location.href = "login.html"; // fallback
        });
}

document.addEventListener("DOMContentLoaded", () => {
    fetchWeeklyPrompt();
    fetchPromptList();
    fetchFeedback();

    const filterBtn = document.getElementById("filter-btn");
    const clearBtn = document.getElementById("clear-filter-btn");

    if (filterBtn) {
        filterBtn.addEventListener("click", () => {
            const dateFrom = document.getElementById("date-from")?.value || "";
            const dateTo = document.getElementById("date-to")?.value || "";
            const promptId = document.getElementById("prompt-select")?.value || "0";
            fetchFilteredFeedback(dateFrom, dateTo, promptId);
        });
    }

    if (clearBtn) {
        clearBtn.addEventListener("click", () => {
            const df = document.getElementById("date-from");
            const dt = document.getElementById("date-to");
            const sel = document.getElementById("prompt-select");
            if (df) df.value = "";
            if (dt) dt.value = "";
            if (sel) sel.value = "0";
            fetchFeedback();
        });
    }
});
