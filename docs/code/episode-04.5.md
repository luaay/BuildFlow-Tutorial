# Episode 04.5 (Bonus) — Solution Structure & GitHub Setup

> **BuildFlow Tutorial**
>
> **Copy → Paste → Run**

---

# Overview

In this episode, you will:

- Create the project root directory
- Build a clean solution structure
- Lock the .NET SDK version
- Create the solution file
- Initialize a Git repository
- Connect the project to GitHub
- Commit and push the initial source code

---

# Part 1 — Solution Structure

## Step 1 — Create the Project Directory

Create the project folder and navigate into it.

```bash
mkdir BuildFlow-Tutorial
cd BuildFlow-Tutorial
```

---

## Step 2 — Create the Solution Folders

Create the primary folders used throughout the project.

```bash
mkdir src
mkdir tests
mkdir docs
```

---

## Step 3 — Lock the .NET SDK Version

Create a `global.json` file to ensure every developer uses the same SDK version.

### Create the file

```bash
dotnet new globaljson --sdk-version 8.0.204
```

### Recommended configuration

```json
{
  "sdk": {
    "version": "8.0.204",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

> **Why?**
>
> Locking the SDK version ensures consistent builds across all development environments and CI/CD pipelines.

---

## Step 4 — Create the Solution

Initialize an empty solution.

```bash
dotnet new sln -n BuildFlow-Tutorial
```

---

## Step 5 — Create the README

Create the initial project documentation.

```bash
echo "# BuildFlow-Tutorial" > README.md
```

---

# Part 2 — Git & GitHub Setup

## Step 6 — Initialize Git

Initialize a local Git repository.

```bash
git init
```

---

## Step 7 — Connect the GitHub Repository

Add the remote GitHub repository.

```bash
git remote add origin https://github.com/luaay/BuildFlow-Tutorial.git
```

---

## Step 8 — Verify the Remote

Verify that the remote repository has been configured correctly.

```bash
git remote -v
```

### Expected Output

```text
origin  https://github.com/luaay/BuildFlow-Tutorial.git (fetch)
origin  https://github.com/luaay/BuildFlow-Tutorial.git (push)
```

---

## Step 9 — Pull Existing Files

If the GitHub repository already contains a README, License, or `.gitignore`, pull them before making your first commit.

```bash
git pull origin main --allow-unrelated-histories
```

---

## Step 10 — Stage All Files

Stage every newly created file.

```bash
git add .
```

---

## Step 11 — Create the First Commit

Commit the initial project structure.

```bash
git commit -m "chore: initialize solution structure and setup project documentation"
```

---

## Step 12 — Push to GitHub

Push the local repository to GitHub.

```bash
git push -u origin main
```

---

# Complete Command List

```bash
# Create project
mkdir BuildFlow-Tutorial
cd BuildFlow-Tutorial

# Create folders
mkdir src
mkdir tests
mkdir docs

# Lock .NET SDK
dotnet new globaljson --sdk-version 8.0.204

# Create solution
dotnet new sln -n BuildFlow-Tutorial

# Create README
echo "# BuildFlow-Tutorial" > README.md

# Initialize Git
git init

# Connect GitHub
git remote add origin https://github.com/luaay/BuildFlow-Tutorial.git

# Verify remote
git remote -v

# Pull existing files
git pull origin main --allow-unrelated-histories

# Stage files
git add .

# First commit
git commit -m "chore: initialize solution structure and setup project documentation"

# Push to GitHub
git push -u origin main
```

---

# Expected Folder Structure

```text
BuildFlow-Tutorial/
│
├── docs/
├── src/
├── tests/
├── README.md
├── global.json
└── BuildFlow-Tutorial.sln
```

---

# Learning Outcome

By the end of this episode, you will have:

- ✅ Created a professional solution structure
- ✅ Organized the project into dedicated folders
- ✅ Locked the .NET SDK version
- ✅ Created the solution file
- ✅ Initialized a Git repository
- ✅ Connected the project to GitHub
- ✅ Created the first commit
- ✅ Successfully pushed the project to GitHub

---

# Episode Completed

**Congratulations!**

You now have a clean, professional, and production-ready project foundation that follows modern .NET development practices and is ready for the upcoming BuildFlow architecture.

```

```