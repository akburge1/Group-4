
function displayPrompt() {
    const promptElement = document.getElementById("weekly-prompt");

    fetch("/ProjectServices.asmx/GetCurrentPrompt", {
        method: "POST",
        headers: { "Content-Type": "application/json" }
    })
        .then(res => res.json())
        .then(data => {
            const prompt = data?.d;
            if (prompt && prompt.text) {
                promptElement.textContent = prompt.Text;
                displayFeedback();
            } else {
                promptElement.textContent = "Unable to load prompt.";
            }
        })
        .catch(() => {
            promptElement.textContent = "Error loading prompt.";
        });
}
function displayFeedback(feedbackList) {
    const tbody = document.getElementById("feedback-list");
    tbody.innerHTML = "";
    feedbackList.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `<td>${item.dateReceived}</td><td>${item.message}</td>`;
        tbody.appendChild(row);
    });
}

document.addEventListener("DOMContentLoaded", displayPrompt);

