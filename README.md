# CIS440 Group 4 Web Application Project: TrueVoice

## 📌 Overview
The **TrueVoice Web Application** is a feedback management platform that allows users to securely log in, view prompts, and submit feedback.  
Administrators can manage prompts, review submissions, and maintain the system through a dedicated admin interface.

The project is built using **ASP.NET Web Forms (C#)** for the backend and **HTML, CSS, and JavaScript** for the frontend. It integrates with a **SQL Server** database for persistent data storage.

---

## ✨ Features
- **User Authentication** – Secure login system for users and administrators.
- **Prompt Management** – Admins can create, edit, and delete prompts.
- **Feedback Submission** – Users can submit responses tied to specific prompts.
- **Database Integration** – SQL Server backend with scripts for table creation and seed data.
- **Responsive Interface** – Clean and user-friendly UI design.

---

## 📂 Project Structure
```
Group-4-main/
│── InsertIntoPrompts.sql          # Script to populate prompt data
│── createtables.sql               # Script to create database tables
│── ProjectTemplate.sln            # Visual Studio solution file
│── ProjectTemplate/
│   ├── App_Start/WebApiConfig.cs  # Web API configuration
│   ├── DatabaseHelper.cs          # Database connection helper
│   ├── ProjectServices.asmx       # Web service definition
│   ├── ProjectServices.asmx.cs    # Web service implementation
│   ├── admin.html / admin.js      # Admin dashboard
│   ├── index.html / index.js      # User dashboard
│   ├── login.html / login.js      # Login interface
│   ├── style.css                  # Application styling
│   ├── logo.png                   # Project logo
│   ├── Web.config                 # Application configuration
│   └── Properties/AssemblyInfo.cs # Assembly metadata
└── README.md                      # Project documentation
```

---

## 🛠️ Installation & Setup

### 1️⃣ Prerequisites
- **Software**
  - Visual Studio 2019 or later
  - SQL Server (Local or Remote)
- **Languages & Frameworks**
  - C#, ASP.NET Web Forms
  - HTML, CSS, JavaScript

### 2️⃣ Database Setup
1. Open **SQL Server Management Studio (SSMS)**.
2. Execute `createtables.sql` to create required tables.
3. Execute `InsertIntoPrompts.sql` to insert initial prompt data.

### 3️⃣ Backend Setup
1. Open `ProjectTemplate.sln` in Visual Studio.
2. Update the database connection string in `Web.config` if necessary.
3. Build and run the project.

### 4️⃣ Accessing the Application
- **User Portal:** Navigate to `index.html` after login.
- **Admin Portal:** Navigate to `admin.html` (requires admin credentials).

---

## 📜 Usage
1. **Login** with provided credentials.
2. **Users** can:
   - View prompts
   - Submit feedback
3. **Admins** can:
   - Manage prompts
   - Review feedback
   - Perform administrative tasks

---

## 🧩 Technologies Used
- **Backend:** C#, ASP.NET Web Forms
- **Frontend:** HTML5, CSS3, JavaScript
- **Database:** Microsoft SQL Server
- **Tools:** Visual Studio, SQL Server Management Studio

---

## 👥 Authors
**Group 4**  
Adam Burgess, Adison Novosel, Brecklyn Gardner, Ellie Lewis, Justin Sakamoto, Madelyn Smith
