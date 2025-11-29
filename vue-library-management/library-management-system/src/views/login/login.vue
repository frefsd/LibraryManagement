<script setup>
import { ref } from 'vue'
import { loginApi } from '@/api/login'
import { ElMessage } from 'element-plus'
import { useRouter, useRoute } from 'vue-router'

let loginForm = ref({ username: '', password: '' })
let router = useRouter()
let route = useRoute()

//登录
const login = async () => {
  const result = await loginApi(loginForm.value)
  if (result.code) {// 登录成功
    ElMessage.success('登录成功')
    localStorage.setItem('loginUser', JSON.stringify(result.data))
    // 跳转到原页面或首页
    const redirect = route.query.redirect || '/index'
    router.push(redirect)
  } else {
    ElMessage.error(result.msg)
  }
}

//取消
const cancel = () => {
  loginForm.value = {
    username: '',
    password: ''
  }
}
</script>

<template>
  <div id="container">
    <!-- 彩色粒子 -->
    <div class="particle"></div>
    <div class="particle"></div>
    <div class="particle"></div>
    <div class="particle"></div>
    <div class="particle"></div>
    
    <div class="login-form">
      <el-form label-width="80px">
        <p class="title">图书管理系统</p>
        <el-form-item label="用户名" prop="username">
          <el-input v-model="loginForm.username" placeholder="请输入用户名"></el-input>
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input type="password" v-model="loginForm.password" placeholder="请输入密码"></el-input>
        </el-form-item>

        <el-form-item>
          <el-button class="button" type="primary" @click="login">登 录</el-button>
          <el-button class="button" type="info" @click="cancel">重 置</el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<style scoped>
#container {
  min-height: 100vh;
  padding: 0;
  background: linear-gradient(
    45deg,
    #ff6b6b,
    #4ecdc4,
    #45b7d1,
    #96ceb4,
    #feca57,
    #ff9ff3,
    #54a0ff
  );
  background-size: 400% 400%;
  animation: gradientShift 8s ease infinite;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

@keyframes gradientShift {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

#container::before {
  content: '';
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: 
    radial-gradient(circle at 20% 80%, rgba(255, 107, 107, 0.3) 0%, transparent 50%),
    radial-gradient(circle at 80% 20%, rgba(78, 205, 196, 0.3) 0%, transparent 50%),
    radial-gradient(circle at 40% 40%, rgba(255, 159, 243, 0.3) 0%, transparent 50%),
    radial-gradient(circle at 60% 60%, rgba(84, 160, 255, 0.3) 0%, transparent 50%);
  animation: float 6s ease-in-out infinite;
}

#container::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: 
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      rgba(255, 255, 255, 0.1) 10px,
      rgba(255, 255, 255, 0.1) 20px
    );
  animation: slide 20s linear infinite;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
  }
}

@keyframes slide {
  0% {
    background-position: 0 0;
  }
  100% {
    background-position: 100px 100px;
  }
}

/* 添加彩色粒子效果 */
.particle {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  animation: particleFloat 8s infinite linear;
}

.particle:nth-child(1) {
  width: 80px;
  height: 80px;
  top: 10%;
  left: 10%;
  background: radial-gradient(circle, #ff6b6b, transparent);
  animation-delay: 0s;
}

.particle:nth-child(2) {
  width: 60px;
  height: 60px;
  top: 20%;
  right: 15%;
  background: radial-gradient(circle, #4ecdc4, transparent);
  animation-delay: 1s;
}

.particle:nth-child(3) {
  width: 100px;
  height: 100px;
  bottom: 15%;
  left: 20%;
  background: radial-gradient(circle, #ff9ff3, transparent);
  animation-delay: 2s;
}

.particle:nth-child(4) {
  width: 70px;
  height: 70px;
  bottom: 25%;
  right: 10%;
  background: radial-gradient(circle, #54a0ff, transparent);
  animation-delay: 3s;
}

.particle:nth-child(5) {
  width: 90px;
  height: 90px;
  top: 50%;
  left: 5%;
  background: radial-gradient(circle, #feca57, transparent);
  animation-delay: 4s;
}

@keyframes particleFloat {
  0%, 100% {
    transform: translateY(0px) rotate(0deg) scale(1);
    opacity: 0.7;
  }
  25% {
    transform: translateY(-40px) rotate(90deg) scale(1.1);
    opacity: 1;
  }
  50% {
    transform: translateY(-20px) rotate(180deg) scale(0.9);
    opacity: 0.8;
  }
  75% {
    transform: translateY(-60px) rotate(270deg) scale(1.2);
    opacity: 0.9;
  }
}

.login-form {
  width: 440px;
  padding: 48px 40px;
  margin: 0;
  border: none;
  border-radius: 20px;
  box-shadow: 
    0 20px 40px rgba(0, 0, 0, 0.15),
    0 8px 24px rgba(0, 0, 0, 0.1),
    inset 0 1px 0 rgba(255, 255, 255, 0.2);
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  position: relative;
  z-index: 10;
  transform: translateY(0);
  transition: all 0.4s ease;
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.login-form:hover {
  transform: translateY(-5px);
  box-shadow: 
    0 25px 50px rgba(0, 0, 0, 0.2),
    0 12px 30px rgba(0, 0, 0, 0.15),
    inset 0 1px 0 rgba(255, 255, 255, 0.2);
}

.title {
  font-size: 36px;
  font-family: "Microsoft YaHei", "PingFang SC", sans-serif;
  text-align: center;
  margin-bottom: 40px;
  font-weight: 800;
  background: linear-gradient(135deg, #ff6b6b, #4ecdc4, #54a0ff, #ff9ff3);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  background-size: 300% 300%;
  animation: titleGradient 4s ease infinite;
  letter-spacing: 1px;
  position: relative;
  padding-bottom: 16px;
}

@keyframes titleGradient {
  0%, 100% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
}

.title::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 80px;
  height: 4px;
  background: linear-gradient(90deg, #ff6b6b, #4ecdc4, #54a0ff);
  border-radius: 2px;
  animation: lineGradient 3s ease infinite;
}

@keyframes lineGradient {
  0%, 100% {
    background: linear-gradient(90deg, #ff6b6b, #4ecdc4, #54a0ff);
  }
  33% {
    background: linear-gradient(90deg, #4ecdc4, #54a0ff, #ff6b6b);
  }
  66% {
    background: linear-gradient(90deg, #54a0ff, #ff6b6b, #4ecdc4);
  }
}

:deep(.el-form) {
  margin-top: 8px;
}

:deep(.el-form-item) {
  margin-bottom: 28px;
}

:deep(.el-form-item__label) {
  font-weight: 600;
  color: #2d3748;
  font-size: 15px;
  padding-right: 16px;
}

:deep(.el-input) {
  border-radius: 12px;
}

:deep(.el-input .el-input__inner) {
  border-radius: 12px;
  border: 2px solid #e2e8f0;
  transition: all 0.3s ease;
  font-size: 15px;
  padding: 12px 16px;
  height: 48px;
  background: #f8fafc;
}

:deep(.el-input .el-input__inner:hover) {
  border-color: #cbd5e0;
  background: #ffffff;
}

:deep(.el-input .el-input__inner:focus) {
  border-color: #667eea;
  background: #ffffff;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  transform: translateY(-1px);
}

:deep(.el-input .el-input__inner::placeholder) {
  color: #a0aec0;
  font-weight: 500;
}

:deep(.el-form-item__content) {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-top: 8px;
}

.button {
  margin-top: 20px;
  width: 140px;
  height: 46px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 15px;
  letter-spacing: 1px;
  transition: all 0.3s ease;
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

:deep(.el-button--primary) {
  background: linear-gradient(135deg, #ff6b6b, #ff9ff3, #54a0ff);
  background-size: 200% 200%;
  animation: buttonGradient 3s ease infinite;
}

@keyframes buttonGradient {
  0%, 100% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
}

:deep(.el-button--primary:hover) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(255, 107, 107, 0.3);
}

:deep(.el-button--info) {
  background: linear-gradient(135deg, #a0aec0, #718096);
  color: white;
}

:deep(.el-button--info:hover) {
  background: linear-gradient(135deg, #718096, #4a5568);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(113, 128, 150, 0.3);
}

/* 响应式设计 */
@media (max-width: 480px) {
  .login-form {
    width: 90%;
    padding: 40px 24px;
    margin: 20px;
  }
  
  .title {
    font-size: 28px;
  }
  
  :deep(.el-form-item__content) {
    flex-direction: column;
    align-items: center;
  }
  
  .button {
    width: 100%;
    max-width: 200px;
  }
}
</style>