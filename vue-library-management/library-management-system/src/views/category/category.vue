<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  queryPageApi,
  addApi,
  queryInfoApi,
  updateApi,
  deleteApi
} from '@/api/category'

const tableData = ref([])
const pagination = ref({ currentPage: 1, pageSize: 10, total: 0 })
const dialogVisible = ref(false)
const formTitle = ref('')
const categoryFormRef = ref()

const category = ref({
  id: '',
  name: '',
  status: 1
})

const rules = ref({
  name: [
    { required: true, message: '分类名称不能为空', trigger: 'blur' },
    { max: 50, message: '最多50个字符', trigger: 'blur' }
  ]
})

onMounted(() => {
  queryPage()
})

const queryPage = async () => {
  const res = await queryPageApi(pagination.value.currentPage, pagination.value.pageSize)
  if (res.code) {
    tableData.value = res.data.rows || []
    pagination.value.total = res.data.total || 0
  }
}

const handleSizeChange = (val) => {
  pagination.value.pageSize = val
  queryPage()
}

const handleCurrentChange = (val) => {
  pagination.value.currentPage = val
  queryPage()
}

const openDialog = (type, id = null) => {
  formTitle.value = type === 'add' ? '新增分类' : '编辑分类'
  dialogVisible.value = true
  if (type === 'add') {
    category.value = { name: '', status: 1 }
    categoryFormRef.value?.resetFields()
  } else {
    loadCategory(id)
  }
}

const loadCategory = async (id) => {
  const res = await queryInfoApi(id)
  if (res.code && res.data) {
    category.value = {
      id: res.data.id,
      name: res.data.name,
      description: res.data.description || '',
      status: res.data.status
    }
  }
}

const save = async () => {
  await categoryFormRef.value.validate()
  const api = category.value.id
    ? updateApi(category.value.id, category.value)
    : addApi(category.value)

  const res = await api
  if (res.code) {
    ElMessage.success('操作成功')
    dialogVisible.value = false
    queryPage()
  }
}

const delCategory = async (id) => {
  try {
    await ElMessageBox.confirm('确认删除？该分类下的图书将无法归类！', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    const res = await deleteApi(id)
    if (res.code) {
      ElMessage.success('删除成功')
      queryPage()
    }
  } catch { }
}

const getStatusText = (status) => (status == 1 ? '启用' : '禁用')
</script>

<template>
  <div class="category-management-container">
    <!-- 标题 -->
    <div class="section-header">
      <h1 class="page-title">分类管理</h1>
    </div>

    <!-- 操作区域 -->
    <div class="section-header">
      <el-button type="success" @click="openDialog('add')" class="add-btn">
        新增分类
      </el-button>
    </div>

    <!-- 表格卡片 -->
    <div class="table-card">
      <el-table :data="tableData" border style="width: 100%" fit class="data-table">
        <el-table-column prop="id" label="ID" width="60" align="center" />
        <el-table-column prop="name" label="分类名称" min-width="180" />
        <el-table-column label="状态" width="100" align="center">
          <template #default="scope">
            {{ getStatusText(scope.row.status) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" align="center" fixed="right">
          <template #default="scope">
            <el-button size="default" type="primary" @click="openDialog('edit', scope.row.id)">编辑</el-button>
            <el-button size="default" type="danger" @click="delCategory(scope.row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="pagination.currentPage"
          v-model:page-size="pagination.pageSize"
          :total="pagination.total"
          :page-sizes="[5, 10, 20]"
          layout="total, sizes, prev, pager, next"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          class="pagination"
          background
        />
      </div>
    </div>

    <!-- 对话框 -->
    <el-dialog v-model="dialogVisible" :title="formTitle" width="400px" class="form-dialog">
      <el-form :model="category" :rules="rules" ref="categoryFormRef" label-width="80px">
        <el-form-item label="分类名称" prop="name">
          <el-input v-model="category.name" placeholder="请输入分类名称" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch
            v-model="category.status"
            :active-value="1"
            :inactive-value="0"
            active-text="启用"
            inactive-text="禁用"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.category-management-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: calc(100vh - 40px);
}

/* 关键：统一样式容器，确保左对齐 */
.section-header {
  padding: 0; /* 不加额外 padding */
  margin-bottom: 20px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
  margin: 0; /* 清除默认 margin */
  line-height: 1.2;
}

.add-btn {
  border-radius: 4px;
  font-weight: 500;
  padding: 10px 20px;
  margin: 0; /* 防止按钮自带 margin */
}

/* 表格卡片 */
.table-card {
  background: #ffffff;
  border-radius: 6px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.data-table :deep(.el-table__header) th {
  background-color: #f5f7fa;
  color: #606266;
  font-weight: 600;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}
</style>