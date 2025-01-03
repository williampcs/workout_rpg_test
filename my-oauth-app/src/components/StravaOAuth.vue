<template>
  <div>
    <h1>Strava OAuth 測試</h1>
    <button @click="startOAuth">連接 Strava</button>
    <div v-if="activities.length > 0">
      <h2>最近的運動記錄：</h2>
      <ul>
        <li v-for="(activity, index) in activities" :key="index">
          {{ activity.name }} - {{ activity.distance }} 公里
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      activities: [], // 用於存放運動數據
    };
  },
  methods: {
    startOAuth() {
      // 跳轉到 Strava 的授權頁面
      const clientId = "YOUR_CLIENT_ID"; // 替換為你的 Strava Client ID
      const redirectUri = "http://localhost:8080/callback"; // 替換為你的回調地址
      const scope = "read,activity:read_all"; // 需要的權限
      const authUrl = `https://www.strava.com/oauth/authorize?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=code&scope=${scope}`;
      window.location.href = authUrl;
    },
  },
};
</script>

<style scoped>
button {
  padding: 10px 20px;
  font-size: 16px;
  margin-top: 20px;
}
</style>
