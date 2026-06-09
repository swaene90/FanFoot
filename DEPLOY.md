# Auto-Deploy to Unraid via GitHub Actions

## Overview

On every push to `master`, a GitHub Actions workflow SSHes into the Unraid server and runs `git pull && docker compose up --build -d`.

## Steps

### 1. Generate an SSH key pair (on your local machine)

```bash
ssh-keygen -t ed25519 -C "github-actions-fanfoot" -f ~/.ssh/fanfoot_deploy
```

This creates two files:
- `~/.ssh/fanfoot_deploy` — private key (goes into GitHub)
- `~/.ssh/fanfoot_deploy.pub` — public key (goes onto the Unraid server)

### 2. Authorize the public key on the Unraid server

SSH into Unraid and append the public key:

```bash
cat ~/.ssh/fanfoot_deploy.pub | ssh <user>@<unraid-ip> "cat >> ~/.ssh/authorized_keys"
```

### 3. Clone the repo on the Unraid server

SSH into Unraid and clone the repo to your preferred path:

```bash
git clone https://github.com/<your-username>/fanfoot.git /mnt/user/appdata/fanfoot
```

### 4. Create the .env file on the Unraid server

The `.env` file is not in git, so it needs to be created manually on the server:

```bash
cat > /mnt/user/appdata/fanfoot/.env <<EOF
ConnectionStrings__DefaultConnection=Host=192.168.0.48;Port=5432;Database=fanfoot;Username=postgres;Password=password
DB_CONNECTION_STRING=Host=192.168.0.48;Port=5432;Database=fanfoot;Username=postgres;Password=password
EOF
```

### 5. Add GitHub Actions secrets

In your GitHub repo go to **Settings → Secrets and variables → Actions** and add:

| Secret name | Value |
|---|---|
| `UNRAID_HOST` | Your Unraid server's IP address |
| `UNRAID_USER` | The SSH username on Unraid |
| `UNRAID_SSH_KEY` | Contents of `~/.ssh/fanfoot_deploy` (the private key) |

### 6. Create the GitHub Actions workflow

Create the file `.github/workflows/deploy.yml` in the repo:

```yaml
name: Deploy to Unraid

on:
  push:
    branches: [master]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Unraid
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.UNRAID_HOST }}
          username: ${{ secrets.UNRAID_USER }}
          key: ${{ secrets.UNRAID_SSH_KEY }}
          script: |
            cd /mnt/user/appdata/fanfoot
            git pull origin master
            docker compose up --build -d
```

Update the `cd` path if you cloned the repo somewhere other than `/mnt/user/appdata/fanfoot`.

### 7. Push and verify

Push any change to `master` and watch the **Actions** tab in GitHub to confirm the workflow runs and the deployment succeeds.
