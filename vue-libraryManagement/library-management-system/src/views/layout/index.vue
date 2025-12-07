<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus';
import { useRouter } from 'vue-router'
import { Document, Grid, Reading } from '@element-plus/icons-vue';

let router = useRouter()

const loginName = ref('')
//定义钩子函数, 获取登录用户名
onMounted(() => {
  //获取登录用户名
  let loginUser = JSON.parse(localStorage.getItem('loginUser'))
  if (loginUser && loginUser.user.realName) {
    loginName.value = loginUser.user.realName
  }
})

const logout = () => {
  //弹出确认框, 如果确认, 则退出登录, 跳转到登录页面
  ElMessageBox.confirm('确认退出登录吗?', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {//确认, 则清空登录信息
    ElMessage.success('退出登录成功')
    localStorage.removeItem('loginUser')
    router.push('/login')//跳转到登录页面
  })
}
</script>

<template>
  <div class="common-layout">
    <el-container>
      <!-- Header 区域 -->
      <el-header class="header">
        <span class="title">图书管理系统</span>
        <span class="right_tool">
          <a href="">
            <el-icon>
              <EditPen />
            </el-icon> 修改密码 &nbsp;&nbsp;&nbsp; | &nbsp;&nbsp;&nbsp;
          </a>
          <a href="javascript:void(0)" @click="logout">
            <el-icon>
              <SwitchButton />
            </el-icon> 退出登录 【{{ loginName }}】
          </a>
        </span>
      </el-header>

      <el-container>
        <!-- 左侧菜单 -->
        <el-aside width="200px" class="aside">
          <el-menu router>
            <!-- 首页菜单 -->
            <el-menu-item index="/index">
              <el-icon>
                <Promotion />
              </el-icon> 首页
            </el-menu-item>
            <!-- 图书信息管理菜单 -->
            <el-sub-menu index="/bookinfo">
              <template #title>
                <el-icon>
                  <Menu />
                </el-icon> 图书信息管理
              </template>
              <el-menu-item index="/book">
                <el-icon>
                  <Reading />
                </el-icon>图书管理
              </el-menu-item>
              <el-menu-item index="/category">
                <el-icon>
                  <Grid />
                </el-icon>分类管理
              </el-menu-item>
              <el-menu-item index="/user">
                <el-icon>
                  <Stamp />
                </el-icon>用户管理
              </el-menu-item>
              <el-menu-item index="/borrow">
                <el-icon>
                  <UserFilled />
                </el-icon>借阅管理
              </el-menu-item>
            </el-sub-menu>

            <!-- 数据统计管理 -->
            <el-sub-menu index="/report">
              <template #title>
                <el-icon>
                  <Histogram />
                </el-icon>数据统计管理
              </template>
              <el-menu-item index="/report/book">
                <el-icon>
                  <InfoFilled />
                </el-icon>图书信息统计
              </el-menu-item>
              <el-menu-item index="/report/category">
                <el-icon>
                  <Share />
                </el-icon>图书分类统计
              </el-menu-item>
              <el-menu-item index="/report/borrow">
                <el-icon>
                  <Document />
                </el-icon>借阅信息统计
              </el-menu-item>
            </el-sub-menu>
          </el-menu>
        </el-aside>

        <!--主展示区域-->
        <el-main>
          <router-view></router-view>
        </el-main>
      </el-container>

    </el-container>
  </div>
</template>

<style scoped>
.common-layout {
  height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
}

.el-container {
  height: 100%;
}

/* Header 区域优化 */
.header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 50%, #667eea 100%);
  background-size: 200% 200%;
  animation: gradientShift 8s ease infinite;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.3);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 40px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
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

.title {
  color: white;
  font-size: 32px;
  font-family: "Microsoft YaHei", "PingFang SC", sans-serif;
  font-weight: 800;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  letter-spacing: 1px;
}

.right_tool {
  display: flex;
  align-items: center;
  gap: 20px;
}

.right_tool a {
  color: white;
  text-decoration: none;
  font-weight: 600;
  font-size: 14px;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 6px;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.right_tool a:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.right_tool .el-icon {
  font-size: 16px;
}

/* 侧边栏优化 */
.aside {
  width: 260px;
  background: linear-gradient(180deg, #ffffff 0%, #f8fafc 100%);
  border-right: 1px solid #e1e8ed;
  box-shadow: 4px 0 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

:deep(.el-menu) {
  border: none;
  background: transparent;
  padding: 16px 8px;
}

:deep(.el-menu-item) {
  height: 52px;
  line-height: 52px;
  margin: 4px 8px;
  border-radius: 10px;
  font-weight: 600;
  color: #4a5568;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 12px;
}

:deep(.el-menu-item:hover) {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  transform: translateX(4px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

:deep(.el-sub-menu__title) {
  height: 52px;
  line-height: 52px;
  margin: 4px 8px;
  border-radius: 10px;
  font-weight: 600;
  color: #4a5568;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 12px;
}

:deep(.el-sub-menu__title:hover) {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  transform: translateX(4px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

:deep(.el-sub-menu .el-menu-item) {
  margin: 2px 8px 2px 20px;
  height: 48px;
  line-height: 48px;
  border-radius: 8px;
  font-weight: 500;
}

:deep(.el-sub-menu .el-menu-item:hover) {
  background: linear-gradient(135deg, #a3bffa, #7e9ffa);
  transform: translateX(4px);
}

:deep(.el-icon) {
  font-size: 18px;
  width: 20px;
}

/* 主内容区域优化 */
.el-main {
  padding: 24px;
  background: #f8fafc;
  overflow: auto;
}

:deep(.el-main > *) {
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  background: white;
  min-height: calc(100vh - 48px);
}

/* 响应式优化 */
@media (max-width: 768px) {
  .header {
    padding: 0 20px;
  }

  .title {
    font-size: 24px;
  }

  .aside {
    width: 200px;
  }

  .right_tool a span {
    display: none;
  }
}

/* 滚动条优化 */
.aside::-webkit-scrollbar {
  width: 6px;
}

.aside::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.aside::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #667eea, #764ba2);
  border-radius: 3px;
}

.aside::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #5a6fd8, #6a4190);
}
</style>