<script setup>
// 删除 Layout 的导入
</script>

<template>
  <!-- 只有一个 router-view，由路由系统决定渲染谁 -->
  <router-view />
</template>

<style>
<!-- 隐藏滚动条-->
html::-webkit-scrollbar,
body::-webkit-scrollbar {
  display: none;
}
html {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>