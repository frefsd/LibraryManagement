<script setup>
import { onMounted, ref } from 'vue'
import {
  queryPageApi,
  addApi,
  queryInfoApi,
  updateApi,
  deleteApi
} from '@/api/user'
import { ElMessage, ElMessageBox } from 'element-plus'

// ============ 搜索相关 ============
const searchUser = ref({
  name: '',
  phone: '',
  cardNumber: ''
})

const tableData = ref([])
const pagination = ref({
  currentPage: 1,
  pageSize: 10,
  total: 0
})

// ============ 表单相关 ============
const dialogFormVisible = ref(false)
const formTitle = ref('')
const labelWidth = ref(100)

const user = ref({
  id: '',
  name: '',
  phone: '',
  email: '',
  cardNumber: '',
  status: 1
})

const userFormRef = ref()

// ============ 状态映射 ============
const getStatusText = (status) => {
  return status === 1 ? '启用' : '禁用'
}

const getStatusType = (status) => {
  return status === 1 ? 'success' : 'info'
}

// ============ 生命周期 ============
onMounted(() => {
  queryPage()
})

// ============ 分页与搜索 ============
const queryPage = async () => {
  try {
    const res = await queryPageApi(
      searchUser.value.name,
      searchUser.value.phone,
      searchUser.value.cardNumber,
      pagination.value.currentPage,
      pagination.value.pageSize
    )

    if (res.code && res.data) {
      tableData.value = res.data.rows || []
      pagination.value.total = res.data.total || 0
    } else {
      ElMessage.warning(res.msg || '查询无数据')
      tableData.value = []
      pagination.value.total = 0
    }
  } catch (error) {
    ElMessage.error('查询失败，请检查网络或联系管理员')
    console.error('查询用户列表出错:', error)
    tableData.value = []
    pagination.value.total = 0
  }
}

const handleSizeChange = (pageSize) => {
  pagination.value.pageSize = pageSize
  queryPage()
}

const handleCurrentChange = (page) => {
  pagination.value.currentPage = page
  queryPage()
}

const clear = () => {
  searchUser.value = { name: '', phone: '', cardNumber: '' }
  queryPage()
}

// ============ 表单操作 ============
const clearUser = () => {
  user.value = {
    id: '',
    name: '',
    phone: '',
    email: '',
    cardNumber: '',
    status: 1
  }
}

const addUser = () => {
  dialogFormVisible.value = true
  formTitle.value = '新增用户'
  clearUser()
  resetForm()
}

const updateUser = async (id) => {
  dialogFormVisible.value = true
  formTitle.value = '编辑用户'

  try {
    const res = await queryInfoApi(id)
    if (res.code && res.data) {
      const data = res.data

      // 安全解析状态
      const statusValue =
        data.status === 0 || data.status === '0' || data.status === false
          ? 0
          : 1

      user.value = {
        id: String(data.id),
        name: data.name ?? '',
        phone: data.phone ?? '',
        email: data.email ?? '',
        cardNumber: data.cardNumber ?? '',
        status: statusValue
      }

      resetForm() // 可选：重置校验状态
    } else {
      ElMessage.error(res.msg || '获取用户信息失败')
      dialogFormVisible.value = false
    }
  } catch (error) {
    ElMessage.error('获取用户信息异常')
    dialogFormVisible.value = false
    console.error('获取用户详情失败:', error)
  }
}

const resetForm = () => {
  if (userFormRef.value) {
    userFormRef.value.resetFields()
  }
}

// ============ 表单校验规则 ============
const rules = ref({
  name: [{ required: true, message: '姓名为必填项', trigger: 'blur' }],
  cardNumber: [{ required: true, message: '借书卡号为必填项', trigger: 'blur' }],
  phone: [{ pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号', trigger: 'blur' }],
  email: [{ type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }]
})

// ============ 保存逻辑（用于新增/编辑） ============
const save = async () => {
  if (!userFormRef.value) return

  try {
    await userFormRef.value.validate()

    const payload = {
      name: user.value.name,
      phone: user.value.phone || null,
      email: user.value.email || null,
      cardNumber: user.value.cardNumber,
      status: Number(user.value.status)
    }

    const userId = Number(user.value.id)
    if (userId) {
      payload.id = userId
    }

    const api = userId ? updateApi(payload) : addApi(payload)
    const res = await api

    if (res.code) {
      ElMessage.success('操作成功')
      dialogFormVisible.value = false
      queryPage()
    } else {
      ElMessage.error(res.msg || '操作失败')

      // 如果是禁用操作且失败，自动恢复为启用状态
      if (payload.status === 0 && res.msg && (res.msg.includes('未归还') || res.msg.includes('无法禁用'))) {
        user.value.status = 1
      }
    }
  } catch (error) {
    // 表单验证失败由 Element Plus 自动处理
  }
}

// ============ 删除用户 ============
const delById = async (id) => {
  try {
    await ElMessageBox.confirm('确认删除此用户？', '提示', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const res = await deleteApi(id)
    if (res.code) {
      ElMessage.success('删除成功')
      queryPage()
    } else {
      ElMessage.error(res.msg || '删除失败')
    }
  } catch (error) {
    // 用户取消时不处理
  }
}
</script>

<template>
  <div class="user-management-container">
    <!-- 标题 -->
    <div class="page-header">
      <h1 class="page-title">用户管理</h1>
    </div>

    <!-- 搜索表单 -->
    <div class="search-card">
      <el-form :inline="true" :model="searchUser" class="search-form">
        <el-form-item label="姓名">
          <el-input size="large" v-model="searchUser.name" placeholder="请输入姓名" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input size="large" v-model="searchUser.phone" placeholder="请输入电话" />
        </el-form-item>
        <el-form-item label="借书卡号">
          <el-input size="large" v-model="searchUser.cardNumber" placeholder="请输入借书卡号" />
        </el-form-item>
        <el-form-item class="search-actions">
          <el-button size="default" type="primary" @click="queryPage">查询</el-button>
          <el-button size="default" type="info" @click="clear">清空</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 操作区域 -->
    <div class="action-area">
      <el-button size="default" type="success" @click="addUser">
        <i class="el-icon-plus"></i> 新增用户
      </el-button>
    </div>

    <!-- 用户列表 -->
    <div class="table-card">
      <el-table :data="tableData" border style="width: 100%" fit>
        <el-table-column label="序号" width="70" align="center">
          <template #default="scope">
            {{ scope.$index + 1 + (pagination.currentPage - 1) * pagination.pageSize }}
          </template>
        </el-table-column>
        <el-table-column prop="name" label="姓名" align="center" min-width="100" />
        <el-table-column prop="phone" label="电话" align="center" min-width="120" />
        <el-table-column prop="email" label="邮箱" align="center" min-width="150" />
        <el-table-column prop="cardNumber" label="借书卡号" align="center" min-width="140" />
        <el-table-column label="状态" align="center" width="100">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.status)" size="small">
              {{ getStatusText(scope.row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" align="center" width="160">
          <template #default="scope">
            {{ scope.row.createTime ? scope.row.createTime.split('T')[0] : '' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" align="center" width="150" fixed="right">
          <template #default="scope">
            <el-space size="default" :align="'center'">
              <el-button type="primary" size="default" @click="updateUser(scope.row.id)">编辑</el-button>
              <el-button type="danger" size="" @click="delById(scope.row.id)">删除</el-button>
            </el-space>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination v-model:current-page="pagination.currentPage" v-model:page-size="pagination.pageSize"
          :page-sizes="[5, 10, 20, 50]" layout="total, sizes, prev, pager, next, jumper" :total="pagination.total"
          @size-change="handleSizeChange" @current-change="handleCurrentChange" />
      </div>
    </div>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="480px">
      <el-form :model="user" ref="userFormRef" :rules="rules" label-position="left" :label-width="labelWidth + 'px'">
        <el-form-item label="姓名" prop="name">
          <el-input v-model="user.name" placeholder="请输入姓名" />
        </el-form-item>
        <el-form-item label="电话" prop="phone">
          <el-input v-model="user.phone" placeholder="请输入电话（可选）" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="user.email" placeholder="请输入邮箱（可选）" />
        </el-form-item>
        <el-form-item label="借书卡号" prop="cardNumber">
          <el-input v-model="user.cardNumber" placeholder="请输入借书卡号" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="user.status" style="width: 100%">
            <el-option :value="1" label="启用" />
            <el-option :value="0" label="禁用" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogFormVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.user-management-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: calc(100vh - 40px);
}

.page-header {
  margin-bottom: 20px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
}

.search-card {
  background: #ffffff;
  border-radius: 6px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.search-form {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  align-items: end;
}

.search-actions .el-button {
  margin-left: 10px;
}

.action-area {
  margin-bottom: 20px;
}

.table-card {
  background: #ffffff;
  border-radius: 6px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>