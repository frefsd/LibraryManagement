<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus';
import { useRouter } from 'vue-router'
import { Check, Coin, Document, Grid, Reading, User } from '@element-plus/icons-vue';

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
.header {
  background-image: linear-gradient(to right, #667eea 0%, #764ba2 100%);
}

.title {
  color: white;
  font-size: 40px;
  font-family: 楷体;
  line-height: 60px;
  font-weight: bolder;
}

.right_tool {
  float: right;
  line-height: 60px;
}

a {
  color: white;
  text-decoration: none;
}

.aside {
  width: 220px;
  border-right: 1px solid #ccc;
  height: 730px;
}
</style>