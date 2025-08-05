

async function fetchWeeklyPrompt() {
    const res = await fetch("/ProjectServices.asmx/GetCurrentPrompt", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
    });

    const data = await res.json();
    const prompt = data.d;

    if (prompt) {
        document.getElementById("weekly-prompt").textContent = prompt.text;
        document.getElementById("week-timeframe").textContent =
            `Week starting ${prompt.weekStartIso}`;
    } else {
        document.getElementById("weekly-prompt").textContent = "No prompt available.";
    }
}

async function fetchFeedback() {
    const res = await fetch("/ProjectServices.asmx/GetAllFeedback", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ page: 1, pageSize: 50 })
    });
    const data = await res.json();
    const feedback = data.d?.items || [];
    renderFeedback(feedback);
}

async function fetchFilteredFeedback(dateFrom, dateTo, promptId) {
    const res = await fetch("/ProjectServices.asmx/GetFilteredFeedback", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            dateFrom: dateFrom || "",
            dateTo: dateTo || "",
            promptId: promptId ? parseInt(promptId) : 0,
            page: 1,
            pageSize: 50
        })
    });
    const data = await res.json();
    const feedback = data.d?.items || [];
    renderFeedback(feedback);
}

function renderFeedback(feedback) {
    const tbody = document.getElementById("feedback-list");
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

document.addEventListener("DOMContentLoaded", () => {
    fetchWeeklyPrompt();
    fetchFeedback();

    // Filter button click
    document.getElementById("filter-btn").addEventListener("click", () => {
        const dateFrom = document.getElementById("date-from").value;
        const dateTo = document.getElementById("date-to").value;
        const promptId = document.getElementById("prompt-id").value;
        fetchFilteredFeedback(dateFrom, dateTo, promptId);
    });

    // Clear filters button click
    document.getElementById("clear-filter-btn").addEventListener("click", () => {
        document.getElementById("date-from").value = "";
        document.getElementById("date-to").value = "";
        document.getElementById("prompt-id").value = "";
        fetchFeedback();
    });
});