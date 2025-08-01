async function fetchWeeklyPrompt() {
    const res = await fetch("/ProjectServices.asmx/GetCurrentPrompt", {
        method: "POST",
        headers: { "Content-Type": "application/json" }
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

    const tbody = document.getElementById("feedback-list");
    tbody.innerHTML = "";
    feedback.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `
      <td>${item.dateSubmitted}</td>
      <td>${item.displayName}</td>
      <td>${item.message}</td>
    `;
        tbody.appendChild(row);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    fetchWeeklyPrompt();
    fetchFeedback();
});
