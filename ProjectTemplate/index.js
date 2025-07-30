function displayPrompt() {
    const promptElement = document.getElementById("weekly-prompt");
    if (promptElement) {
        promptElement.textContent = getWeeklyPrompt();
    }
    fetch("/ProjectServices.asmx/GetWeeklyPrompt", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ weekNumber: 1 }) // example week
    })
        .then(res => res.json())
        .then(data => console.log(data.d)); // .d = SOAP response data

}

function handleFormSubmission() {
    const form = document.querySelector(".feedback-form");

    form.addEventListener("submit", function (e) {
        e.preventDefault(); // Prevent actual HTTP POST
        alert("Feedback submitted!"); //testing
        form.reset();
    });
}

document.addEventListener("DOMContentLoaded", () => {
    displayPrompt();
    handleFormSubmission();
});
