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
      activities: [],
    };
  },
  methods: {
    startOAuth() {
      const clientId = "144190";
      const redirectUri = "http://35.212.232.36:8080";
      const scope = "read,activity:read_all";
      const authUrl = `https://www.strava.com/oauth/authorize?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=code&scope=${scope}`;
      window.location.href = authUrl;
    },

    async handleAuthCallback() {
      // 從 URL 獲取 code
      const urlParams = new URLSearchParams(window.location.search);
      const code = urlParams.get("code");

      if (code) {
        try {
          const response = await fetch(
            "http://35.212.232.36:5284/api/OAuth/exchange-token",
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
              },
              body: JSON.stringify({ code }),
            }
          );

          if (!response.ok) {
            throw new Error("Token exchange failed");
          }

          const data = await response.json();
          console.log("Token exchange successful:", data);
          // 這裡可以處理成功後的邏輯，例如獲取運動數據
        } catch (error) {
          console.error("Error exchanging token:", error);
        }
      }
    },
  },
  mounted() {
    // 在組件掛載時檢查 URL 是否包含 code
    this.handleAuthCallback();
  },
};
</script>
