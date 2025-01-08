# --- 第一步：Build 階段，用 Node.js 建置 Vue 前端 ---
FROM node:20-alpine AS build

# 設定容器內的工作目錄
WORKDIR /app

# 把 package.json / package-lock.json 複製到容器
COPY package.json package-lock.json ./

# 安裝相依套件
RUN npm install

# 複製專案其他檔案
COPY . .

# 建置 (build) 專案
RUN npm run build


# --- 第二步：Production 階段，用 Nginx 供靜態檔案 ---
FROM nginx:alpine

# 從 build 階段複製產生的靜態檔 (dist) 到 Nginx 預設的靜態目錄
COPY --from=build /app/dist /usr/share/nginx/html

# 監聽容器內部的 80 port (預設 HTTP)
EXPOSE 80

# 啟動 Nginx
CMD ["nginx", "-g", "daemon off;"]
    