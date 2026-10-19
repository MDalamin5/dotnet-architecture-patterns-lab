# ==========================================
# 1. DEPENDENCIES STAGE (Node 22 Alpine)
# ==========================================
FROM node:22-alpine AS deps
WORKDIR /app

# Copy package descriptors
COPY package*.json ./

# Use npm install to gracefully handle dependencies
RUN npm install

# ==========================================
# 2. BUILDER STAGE
# ==========================================
FROM node:22-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

# ==========================================
# 3. PRODUCTION RUNNER STAGE
# ==========================================
FROM node:22-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Copy standalone build outputs
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000

CMD ["node", "server.js"]