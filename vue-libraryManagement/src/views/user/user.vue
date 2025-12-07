<script setup>
const loading = ref(false)

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
  loading.value = true

  try {
    const res = await queryPageApi(
      searchUser.value.name,
      searchUser.value.phone,
      searchUser.value.cardNumber,
      pagination.value.currentPage,
      pagination.value.pageSize
    )
    loading.value = false

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
  isEditing.value = false
  clearUser()
  resetForm()
}

const isEditing = ref(false) //用于判断当前是编辑模式

const updateUser = async (id) => {
  dialogFormVisible.value = true
  formTitle.value = '编辑用户'
  isEditing.value = true //编辑用户信息时不应许修改姓名

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
          <el-button type="primary" @click="queryPage">查询</el-button>
          <el-button type="info" @click="clear">清空</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 操作区域 -->
    <div class="action-area">
      <el-button type="success" @click="addUser">
        <i class="el-icon-plus"></i> 新增用户
      </el-button>
    </div>

    <!-- 用户列表 -->
    <div class="table-card" v-loading="loading">
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
              <el-button type="danger" size="default" @click="delById(scope.row.id)">删除</el-button>
            </el-space>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination v-model:current-page="pagination.currentPage" v-model:page-size="pagination.pageSize"
          :page-sizes="[5, 10, 20, 50]" layout="total, sizes, prev, pager, next, jumper" :total="pagination.total"
          @size-change="handleSizeChange" @current-change="handleCurrentChange" class="pagination" background />
      </div>
    </div>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="520px" :close-on-click-modal="false" align-center
      class="user-form-dialog">
      <div class="dialog-content">
        <el-form :model="user" ref="userFormRef" :rules="rules" label-position="right" :label-width="'90px'"
          class="user-form">
          <el-form-item label="姓名" prop="name">
            <el-input v-model="user.name" placeholder="请输入姓名" size="large" clearable :disabled="isEditing" />
          </el-form-item>

          <el-form-item label="电话" prop="phone">
            <el-input v-model="user.phone" placeholder="请输入电话" size="large" clearable />
          </el-form-item>

          <el-form-item label="邮箱" prop="email">
            <el-input v-model="user.email" placeholder="请输入邮箱" size="large" clearable />
          </el-form-item>

          <el-form-item label="借书卡号" prop="cardNumber">
            <el-input v-model="user.cardNumber" placeholder="请输入借书卡号" size="large" clearable />
          </el-form-item>

          <el-form-item label="状态" prop="status">
            <el-select v-model="user.status" size="large" style="width: 100%">
              <el-option :value="1" label="启用" />
              <el-option :value="0" label="禁用" />
            </el-select>
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogFormVisible = false" size="large">取消</el-button>
          <el-button type="primary" @click="save" size="large">保存</el-button>
        </div>
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

/* 分页容器样式 */
.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 30px;
  padding: 20px 0;
}

/* 大型分页样式 */
.pagination :deep(.el-pagination) {
  --el-pagination-bg-color: #f5f7fa;
  --el-pagination-button-bg-color: #ffffff;
  --el-pagination-button-disabled-bg-color: #f5f7fa;
  --el-pagination-hover-color: #409EFF;
}

.pagination :deep(.el-pagination.is-background) {
  padding: 8px 16px;
  border-radius: 12px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.pagination :deep(.el-pagination.is-background .btn-prev),
.pagination :deep(.el-pagination.is-background .btn-next),
.pagination :deep(.el-pagination.is-background .el-pager li) {
  background-color: rgba(255, 255, 255, 0.9);
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  color: #333;
  font-weight: 600;
  min-width: 42px;
  height: 42px;
  margin: 0 4px;
  transition: all 0.3s ease;
}

.pagination :deep(.el-pagination.is-background .btn-prev:hover),
.pagination :deep(.el-pagination.is-background .btn-next:hover),
.pagination :deep(.el-pagination.is-background .el-pager li:hover) {
  background-color: #ffffff;
  border-color: #409EFF;
  color: #409EFF;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.3);
}

.pagination :deep(.el-pagination.is-background .btn-prev:disabled),
.pagination :deep(.el-pagination.is-background .btn-next:disabled) {
  background-color: rgba(255, 255, 255, 0.5);
  border-color: rgba(255, 255, 255, 0.2);
  color: #ccc;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.pagination :deep(.el-pagination.is-background .el-pager li.active) {
  background: linear-gradient(135deg, #409EFF, #3375b9);
  border-color: #409EFF;
  color: #ffffff;
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(64, 158, 255, 0.4);
}

.pagination :deep(.el-pagination.is-background .el-pager li.active:hover) {
  background: linear-gradient(135deg, #3375b9, #2661a3);
  border-color: #3375b9;
  color: #ffffff;
}

/* 对话框样式 */
.user-form-dialog {
  border-radius: 8px;
}

.user-form-dialog :deep(.el-dialog__header) {
  padding: 20px 20px 10px;
  margin: 0;
  border-bottom: 1px solid #f0f0f0;
}

.user-form-dialog :deep(.el-dialog__title) {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
}

.user-form-dialog :deep(.el-dialog__body) {
  padding: 20px;
}

.dialog-content {
  padding: 10px 0;
  display: flex;
}

.user-form {
  width: 100%;
}

.user-form :deep(.el-form-item) {
  margin-bottom: 20px;
}

.user-form :deep(.el-form-item__label) {
  font-weight: 500;
  color: #606266;
  padding-right: 12px;
}

.user-form :deep(.el-input__wrapper) {
  border-radius: 6px;
}

.user-form :deep(.el-select) {
  width: 100%;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 10px 0 0;
}

.user-form-dialog :deep(.el-dialog__footer) {
  padding: 0 20px 20px;
  border-top: none;
}
</style>