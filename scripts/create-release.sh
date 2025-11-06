#!/bin/bash

# Script to create a new release
# Usage: ./scripts/create-release.sh [major|minor|patch] [--pre-release]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT_FILE="src/Fermat.EntityFramework.Shared/Fermat.EntityFramework.Shared.csproj"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo -e "${RED}Error: GitHub CLI (gh) is not installed.${NC}"
    echo "Please install it from: https://cli.github.com/"
    exit 1
fi

# Check if user is authenticated with GitHub
if ! gh auth status &> /dev/null; then
    echo -e "${RED}Error: Not authenticated with GitHub.${NC}"
    echo "Please run: gh auth login"
    exit 1
fi

# Get current version from .csproj file (macOS and Linux compatible)
CURRENT_VERSION=$(grep -oE '<Version>[0-9.]+</Version>' "$PROJECT_FILE" | sed -E 's/<Version>([0-9.]+)<\/Version>/\1/' || echo "0.0.0")
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]:-0}
MINOR=${VERSION_PARTS[1]:-0}
PATCH=${VERSION_PARTS[2]:-0}

# Determine version bump type
BUMP_TYPE=${1:-patch}
PRE_RELEASE=${2:-""}

# Ensure version parts are numbers
MAJOR=${MAJOR:-0}
MINOR=${MINOR:-0}
PATCH=${PATCH:-0}

case $BUMP_TYPE in
    major)
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        ;;
    minor)
        MINOR=$((MINOR + 1))
        PATCH=0
        ;;
    patch)
        PATCH=$((PATCH + 1))
        ;;
    *)
        echo -e "${RED}Error: Invalid bump type. Use: major, minor, or patch${NC}"
        exit 1
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
TAG_NAME="v$NEW_VERSION"

# Confirm before proceeding
echo -e "${YELLOW}Current version: $CURRENT_VERSION${NC}"
echo -e "${GREEN}New version: $NEW_VERSION${NC}"
echo -e "${GREEN}Tag name: $TAG_NAME${NC}"
if [ "$PRE_RELEASE" == "--pre-release" ]; then
    echo -e "${YELLOW}This will be a pre-release${NC}"
fi
echo ""
read -p "Do you want to proceed? (y/N) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 0
fi

# Check if working directory is clean
if [[ -n $(git status --porcelain) ]]; then
    echo -e "${YELLOW}Warning: Working directory is not clean.${NC}"
    read -p "Do you want to continue anyway? (y/N) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 0
    fi
fi

# Update version in .csproj file
echo -e "${GREEN}Updating version in $PROJECT_FILE...${NC}"
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    sed -i '' "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$PROJECT_FILE"
else
    # Linux
    sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$PROJECT_FILE"
fi

# Commit the version change
echo -e "${GREEN}Committing version change...${NC}"
git add "$PROJECT_FILE"
git commit -m "Bump version to $NEW_VERSION" || echo "Nothing to commit (version may already be updated)"

# Create git tag
echo -e "${GREEN}Creating git tag: $TAG_NAME...${NC}"
if git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
    echo -e "${RED}Error: Tag $TAG_NAME already exists${NC}"
    exit 1
fi

git tag -a "$TAG_NAME" -m "Release $TAG_NAME"

# Push changes and tags
echo -e "${GREEN}Pushing changes and tags to remote...${NC}"
# Check if upstream is set, if not set it automatically
if ! git rev-parse --abbrev-ref --symbolic-full-name @{u} &>/dev/null; then
    echo -e "${YELLOW}Setting upstream branch...${NC}"
    git push --set-upstream origin $(git branch --show-current)
else
    git push
fi
git push --tags

# Create GitHub release
echo -e "${GREEN}Creating GitHub release...${NC}"
if [ "$PRE_RELEASE" == "--pre-release" ]; then
    gh release create "$TAG_NAME" \
        --title "Release $TAG_NAME" \
        --notes "Release $TAG_NAME" \
        --prerelease
else
    gh release create "$TAG_NAME" \
        --title "Release $TAG_NAME" \
        --notes "Release $TAG_NAME"
fi

echo -e "${GREEN}✓ Release $TAG_NAME created successfully!${NC}"
echo -e "${YELLOW}The GitHub Action will now build, test, and publish the package to NuGet.${NC}"

