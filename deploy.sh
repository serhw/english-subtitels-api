#!/bin/bash

# Deployment script for Subtitles API
# Usage: ./deploy.sh [image_tag] [environment]

set -e

# Default values
IMAGE_TAG="${1:-latest}"
ENVIRONMENT="${2:-production}"
GITHUB_USERNAME="${GITHUB_USERNAME:-your-github-username}"
IMAGE_NAME="ghcr.io/${GITHUB_USERNAME}/english-subtitels-api:${IMAGE_TAG}"

echo "🚀 Deploying Subtitles API"
echo "Image: ${IMAGE_NAME}"
echo "Environment: ${ENVIRONMENT}"
echo ""

# Check if running in Docker Swarm mode
if docker info --format '{{.Swarm.LocalNodeState}}' | grep -q 'active'; then
    echo "📦 Deploying to Docker Swarm..."
    
    # Pull the latest image
    echo "📥 Pulling image: ${IMAGE_NAME}"
    docker pull "${IMAGE_NAME}"
    
    # Create secrets if they don't exist
    echo "🔐 Setting up secrets..."
    if ! docker secret ls --format "table {{.Name}}" | grep -q "subtitles_seq_api_key"; then
        echo "Creating seq_api_key secret (empty by default)..."
        echo "" | docker secret create subtitles_seq_api_key -
    fi
    
    if ! docker secret ls --format "table {{.Name}}" | grep -q "subtitles_db_connection"; then
        echo "Creating db_connection secret (empty by default)..."
        echo "" | docker secret create subtitles_db_connection -
    fi
    
    # Deploy the stack
    echo "🐳 Deploying stack..."
    export DOCKER_IMAGE="${IMAGE_NAME}"
    docker stack deploy -c docker-compose.production.yml subtitles-stack
    
    echo "✅ Deployment complete!"
    echo ""
    echo "📊 Service status:"
    docker service ls --filter name=subtitles-stack
    
    echo ""
    echo "📝 View logs:"
    echo "docker service logs -f subtitles-stack_subtitles-api"
    
else
    echo "📦 Deploying with Docker Compose..."
    
    # Pull the latest image
    echo "📥 Pulling image: ${IMAGE_NAME}"
    docker pull "${IMAGE_NAME}"
    
    # Deploy with docker-compose
    export DOCKER_IMAGE="${IMAGE_NAME}"
    docker-compose -f docker-compose.production.yml up -d
    
    echo "✅ Deployment complete!"
    echo ""
    echo "📊 Container status:"
    docker-compose -f docker-compose.production.yml ps
    
    echo ""
    echo "📝 View logs:"
    echo "docker-compose -f docker-compose.production.yml logs -f subtitles-api"
fi

echo ""
echo "🌐 API should be available at: http://localhost"
echo "📊 Seq logs available at: http://localhost:5341"
