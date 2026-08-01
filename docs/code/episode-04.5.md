# Episode 04.5 (Bonus) — Git Repository Setup & GitHub Integration

> BuildFlow-Tutorial
>
> Copy → Paste → Run

---

# Step 1 — Initialize Git Repository

Open Terminal in the project root directory

```bash
git init

```

---

# Step 2 — Add Remote Repository

```bash
git remote add origin [https://github.com/luaay/BuildFlow-Tutorial.git](https://github.com/luaay/BuildFlow-Tutorial.git)

```

---

# Step 3 — Verify Remote Configuration

```bash
git remote -v

```

Expected Result

```text
origin  [https://github.com/luaay/BuildFlow-Tutorial.git](https://github.com/luaay/BuildFlow-Tutorial.git) (fetch)
origin  [https://github.com/luaay/BuildFlow-Tutorial.git](https://github.com/luaay/BuildFlow-Tutorial.git) (push)

```

---

# Step 4 — Pull Initial Files from GitHub

```bash
git pull origin main --allow-unrelated-histories

```

---

# Step 5 — Stage Changes

```bash
git add .

```

---

# Step 6 — Commit Changes

```bash
git commit -m "chore: initialize solution structure and setup project documentation"

```

---

# Step 7 — Push to GitHub

```bash
git push -u origin main

```

---

# Commands Used

```bash
git init

git remote add origin [https://github.com/luaay/BuildFlow-Tutorial.git](https://github.com/luaay/BuildFlow-Tutorial.git)

git remote -v

git pull origin main --allow-unrelated-histories

git add .

git commit -m "chore: initialize solution structure and setup project documentation"

git push -u origin main

```

---

# Episode Completed

✅ Git Initialized

✅ Remote Linked

✅ Initial Files Pulled

✅ Changes Committed

✅ Pushed to GitHub

```

```