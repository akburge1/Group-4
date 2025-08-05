async function fetchWeeklyPrompt() {
    try {
        const res = await fetch("/ProjectServices.asmx/GetCurrentPrompt", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: "{}"
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
    } catch (err) {
        console.error("Error fetching weekly prompt:", err);
        document.getElementById("weekly-prompt").textContent = "Error loading prompt.";
    }
}

async function fetchFeedback() {
    try {
        const res = await fetch("/ProjectServices.asmx/GetAllFeedback", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ page: 1, pageSize: 50 })
        });

        const data = await res.json();

        if (data.d?.error === "unauthorized") {
            document.getElementById("feedback-list").innerHTML =
                `<tr><td colspan="3">Unauthorized — please log in as admin.</td></tr>`;
            return;
        }

        const feedback = data.d?.items || [];

        const tbody = document.getElementById("feedback-list");
        tbody.innerHTML = "";

        if (feedback.length === 0) {
            tbody.innerHTML = `<tr><td colspan="3">No feedback available.</td></tr>`;
            return;
        }

        feedback.forEach(item => {
            const formattedDate = formatDate(item.dateSubmitted);
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${formattedDate}</td>
                <td>${item.displayName}</td>
                <td>${item.promptText}</td>
                <td>${item.message}</td>
            `;
            tbody.appendChild(row);
        });
    } catch (err) {
        console.error("Error fetching feedback:", err);
        document.getElementById("feedback-list").innerHTML =
            `<tr><td colspan="3">Error loading feedback.</td></tr>`;
    }
}

function formatDate(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date)) return dateString;
    return date.toLocaleString(undefined, {
        year: "numeric",
        month: "short",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    });
}

document.addEventListener("DOMContentLoaded", () => {
    fetchWeeklyPrompt();
    fetchFeedback();
});
