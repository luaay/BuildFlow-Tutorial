# Quick Episode 01 — Solution Structure & GitHub Setup

> **BuildFlow Fast-Track**  
> **Copy → Paste → Run**

---

## 1. Quick Commands Setup

Run the following commands sequentially in your terminal to create the structure, lock the SDK, initialize Git, and push to GitHub:

```bash
# 1. Create project & folders
mkdir BuildFlow-Tutorial
cd BuildFlow-Tutorial
mkdir src tests docs

# 2. Lock .NET SDK version
dotnet new globaljson --sdk-version 8.0.204

# 3. Create solution & README
dotnet new sln -n BuildFlow-Tutorial
echo "# BuildFlow-Tutorial" > README.md

# 4. Initialize Git & Connect Remote
git init
git remote add origin https://github.com/luaay/BuildFlow-Tutorial.git
git remote -v

# 5. Pull, Commit & Push
git pull origin main --allow-unrelated-histories
git add .
git commit -m "chore: initialize solution structure and setup project documentation"
git push -u origin main
```

---

## 2. Expected Folder Structure

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