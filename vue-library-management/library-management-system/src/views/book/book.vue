<script setup>
const loading = ref(false)

import { onMounted, ref, watch } from 'vue'
import {
  queryPageApi,
  addApi,
  queryInfoApi,
  updateApi,
  deleteApi
} from '@/api/book'
import { queryAllApi as queryAllCategoryApi } from '@/api/category'
import { queryAllApi as queryAllPublisherApi } from '@/api/publisher'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { tr } from 'element-plus/es/locales.mjs'

// ============ 搜索相关 ============
const searchBook = ref({
  begin: '',
  end: '',
  date: [],
  name: ''
})

const tableData = ref([])
const pagination = ref({
  currentPage: 1,
  pageSize: 10,
  total: 0
})

// ============ 下拉数据 ============
const categories = ref([])
const publishers = ref([])

// ============ 表单相关 ============
const dialogFormVisible = ref(false)
const formTitle = ref('')
const labelWidth = ref(100)

const book = ref({
  id: '',
  name: '',
  author: '',
  publishDate: '',
  price: '',
  category: '',
  publisherId: '',
  status: 1,
  coverFile: null
})

const bookFormRef = ref()
const originalStatus = ref(1)

// ============ 封面上传相关（必须在顶层） ============
const coverFileList = ref([]) // 用于 el-upload 的 fileList
const coverPreviewUrl = ref('')
const coverPreviewVisible = ref(false)

// ============ 状态映射 ============
const getStatusText = (status) => {
  return status === 1 ? '在库' : status === 2 ? '已下架' : '未知'
}

// ============ 生命周期 ============
onMounted(() => {
  queryPage()
  loadCategories()
  loadPublishers()
})

// ============ 加载分类 & 出版社 ============
const loadCategories = async () => {
  const res = await queryAllCategoryApi()
  if (res.code) {
    categories.value = res.data
  }
}

const loadPublishers = async () => {
  const res = await queryAllPublisherApi()
  if (res.code) {
    publishers.value = res.data
  }
}

// ============ 分页与搜索 ============
const queryPage = async () => {
  loading.value = true
  const result = await queryPageApi(
    searchBook.value.begin,
    searchBook.value.end,
    searchBook.value.name,
    pagination.value.currentPage,
    pagination.value.pageSize
  )
  loading.value = false

  if (result.code) {
    tableData.value = result.data.rows || []
    pagination.value.total = result.data.total || 0
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
  searchBook.value = { begin: '', end: '', date: [], name: '' }
  queryPage()
}

watch(() => searchBook.value.date, (newVal) => {
  if (newVal && newVal.length === 2) {
    searchBook.value.begin = newVal[0]
    searchBook.value.end = newVal[1]
  } else {
    searchBook.value.begin = ''
    searchBook.value.end = ''
  }
})

// ============ 表单操作 ============
const clearBook = () => {
  book.value = {
    id: '',
    name: '',
    author: '',
    publishDate: '',
    price: '',
    category: '',
    publisherId: '',
    status: 1,
    coverFile: null
  }
  coverFileList.value = [] // 同步清空封面
}

const addBook = () => {
  dialogFormVisible.value = true
  formTitle.value = '新增图书'
  isEditing.value = false //新增时可输入完整图书信息
  clearBook()
  resetForm()
}

const isEditing = ref(false) //用于判断当前是编辑模式

const updateBook = async (id) => {
  clearBook()
  dialogFormVisible.value = true
  formTitle.value = '编辑图书'
  isEditing.value = true //编辑图书信息时不应许修改图书名称和作者

  const result = await queryInfoApi(id)
  if (result.code && result.data) {
    const data = result.data

    const categoryId = data.category?.id ?? ''
    const publisherId = data.publisher?.id ?? ''

    let publishDate = ''
    if (data.publishDate) {
      publishDate = data.publishDate.split('T')[0]
    }

    book.value = {
      id: String(data.id ?? ''),
      name: data.name ?? '',
      author: data.author ?? '',
      publishDate,
      price: String(data.price ?? ''),
      category: String(categoryId),
      publisherId: String(publisherId),
      status: data.status ?? 1
    }

    originalStatus.value = data.status ?? 1 // 保存原始状态用于回滚

    // 初始化封面预览
    if (data.coverUrl) {
      coverFileList.value = [{ name: 'cover.jpg', url: data.coverUrl }]
    } else {
      coverFileList.value = []
    }
    book.value.coverFile = null // 默认不传新文件
  }
}

const resetForm = () => {
  if (bookFormRef.value) {
    bookFormRef.value.resetFields()
  }
}

// ============ 封面处理 ============
const handleCoverUploadChange = (uploadFile, uploadFiles) => {
  if (uploadFile.raw && uploadFile.raw.type.startsWith('image/')) {
    book.value.coverFile = uploadFile.raw
    coverFileList.value = uploadFiles.slice(-1) // 只保留一个
  } else {
    ElMessage.warning('请上传图片文件（JPG/PNG/GIF）')
    book.value.coverFile = null
    coverFileList.value = []
  }
}

const handleCoverRemove = () => {
  book.value.coverFile = null
  coverFileList.value = []
}

const handleCoverPreview = (uploadFile) => {
  coverPreviewUrl.value = uploadFile.url || URL.createObjectURL(uploadFile.raw)
  coverPreviewVisible.value = true
}


// ============ 表单校验规则 ============
const rules = ref({
  name: [
    { required: true, message: '图书名称为必填项', trigger: 'blur' },
    { min: 1, max: 100, message: '长度为1-100个字', trigger: 'blur' }
  ],
  author: [
    { required: true, message: '作者为必填项', trigger: 'blur' },
    { min: 1, max: 50, message: '长度为1-50个字', trigger: 'blur' }
  ],
  publishDate: [{ required: true, message: '出版日期为必填项', trigger: 'change' }],
  price: [
    { required: true, message: '价格为必填项', trigger: 'blur' },
    { pattern: /^\d+(\.\d{1,2})?$/, message: '格式如：99.99', trigger: 'blur' }
  ],
  category: [{ required: true, message: '请选择分类', trigger: 'change' }],
  publisherId: [{ required: true, message: '请选择出版社', trigger: 'change' }]
})

// ============ 保存逻辑 ============
const save = async () => {
  if (!bookFormRef.value) return

  try {
    await bookFormRef.value.validate()

    const categoryIdStr = book.value.category.trim()
    const publisherIdStr = book.value.publisherId.trim()

    const categoryId = parseInt(categoryIdStr)
    const publisherId = parseInt(publisherIdStr)

    if (isNaN(categoryId) || categoryId <= 0) {
      ElMessage.warning('请选择有效的图书分类')
      return
    }
    if (isNaN(publisherId) || publisherId <= 0) {
      ElMessage.warning('请选择有效的出版社')
      return
    }

    const formData = new FormData()
    formData.append('name', book.value.name)
    formData.append('author', book.value.author)
    formData.append('publishDate', book.value.publishDate)
    formData.append('price', book.value.price)
    formData.append('categoryId', categoryId)
    formData.append('publisherId', publisherId)
    formData.append('status', book.value.status)

    const bookId = parseInt(book.value.id)
    if (!isNaN(bookId) && bookId > 0) {
      formData.append('id', bookId)
    }

    // 仅当用户选择了新封面才上传
    if (book.value.coverFile) {
      formData.append('coverFile', book.value.coverFile)
    }

    const api = bookId > 0 ? updateApi(formData) : addApi(formData)
    const result = await api

    if (result.code) {
      ElMessage.success('操作成功')
      dialogFormVisible.value = false
      queryPage()
    } else {
      ElMessage.error(result.msg || '操作失败')
      // 如果是因为“无法下架”而失败，回滚状态
      if (result.msg?.includes('无法下架') && book.value.status === 2 && bookId > 0) {
        book.value.status = originalStatus.value
      }
    }
  } catch (error) {
    console.error('保存失败:', error)
  }
}

// ============ 删除 ============
const delById = async (id) => {
  try {
    await ElMessageBox.confirm('确认删除此图书？', '提示', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const result = await deleteApi(id)
    if (result?.code) {
      ElMessage.success('删除成功')
      queryPage()
    } else {
      ElMessage.error(result?.msg || '删除失败')
    }
  } catch (error) {
    if (error === 'cancel') return
  }
}

// ============ 日期格式化 ============
const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'
  const pad = (num) => String(num).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'
  const pad = (num) => String(num).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
}
</script>

<template>
  <div class="book-management-container">
    <!-- 标题 -->
    <div class="page-header">
      <h1 class="page-title">图书管理</h1>
    </div>

    <!-- 搜索表单 -->
    <div class="search-card">
      <el-form :inline="true" :model="searchBook" class="search-form">
        <el-form-item label="图书名称">
          <el-input size="large" v-model="searchBook.name" placeholder="请输入图书名称" class="search-input" />
        </el-form-item>

        <el-form-item label="出版日期">
          <el-date-picker v-model="searchBook.date" type="daterange" range-separator=" 至 " start-placeholder="开始时间"
            end-placeholder="结束时间" value-format="YYYY-MM-DD" class="date-picker" />
        </el-form-item>

        <el-form-item class="search-actions">
          <el-button size="plain" type="primary" @click="queryPage" class="search-btn">查询</el-button>
          <el-button size="plain" type="info" @click="clear" class="clear-btn">清空</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 操作区域 -->
    <div class="action-area">
      <el-button size="plain" type="success" @click="addBook" class="add-btn">
        <i class="el-icon-plus"></i> 新增图书
      </el-button>
    </div>

    <!-- 图书列表 -->
    <div class="table-card" v-loading="loading">
      <el-table :data="tableData" border style="width: 100%" fit class="data-table">
        <el-table-column label="序号" width="70" align="center">
          <template #default="scope">
            {{ scope.$index + 1 + (pagination.currentPage - 1) * pagination.pageSize }}
          </template>
        </el-table-column>
        <el-table-column label="图书封面" align="center" width="120">
          <template #default="scope">
            <img v-if="scope.row.coverUrl" :src="scope.row.coverUrl" alt="封面" class="book-cover"
              @error="() => (scope.row.coverUrl = null)" />
            <div v-else class="book-cover-placeholder">
              <el-icon>
                <Plus />
              </el-icon>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="name" label="图书名称" align="center" min-width="180" />
        <el-table-column prop="author" label="作者" align="center" width="110" />
        <el-table-column label="分类" align="center" width="110">
          <template #default="scope">
            <el-tag type="info" size="plain">{{ scope.row.category?.name || '未知' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="出版日期" align="center" width="130">
          <template #default="scope">
            {{ formatDate(scope.row.publishDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="price" label="价格" align="center" width="100">
          <template #default="scope">
            <span class="price">¥{{ scope.row.price }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" align="center" width="100">
          <template #default="scope">
            <el-tag :type="scope.row.status === 1 ? 'success' : 'info'" size="plain">
              {{ getStatusText(scope.row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="最后修改时间" align="center" width="160">
          <template #default="scope">
            {{ formatDateTime(scope.row.updateTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" align="center" width="160" fixed="right">
          <template #default="scope">
            <el-button type="primary" size="plain" @click="updateBook(scope.row.id)" class="action-btn">编辑</el-button>
            <el-button type="danger" size="plain" @click="delById(scope.row.id)" class="action-btn">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination v-model:current-page="pagination.currentPage" v-model:page-size="pagination.pageSize"
          :page-sizes="[5, 10, 20, 50, 100]" layout="total, sizes, prev, pager, next, jumper" :total="pagination.total"
          @size-change="handleSizeChange" @current-change="handleCurrentChange" class="pagination" background />
      </div>
    </div>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="450px" class="form-dialog">
      <el-form :model="book" ref="bookFormRef" :rules="rules" label-position="left">
        <el-form-item label="图书名称" :label-width="labelWidth" prop="name">
          <el-input v-model="book.name" placeholder="请输入图书名称" :disabled="isEditing" />
        </el-form-item>

        <el-form-item label="作者" :label-width="labelWidth" prop="author">
          <el-input v-model="book.author" placeholder="请输入作者" :disabled="isEditing"/>
        </el-form-item>

        <el-form-item label="出版日期" :label-width="labelWidth" prop="publishDate">
          <el-date-picker v-model="book.publishDate" type="date" placeholder="请选择出版日期" value-format="YYYY-MM-DD"
            style="width: 100%" />
        </el-form-item>

        <el-form-item label="价格" :label-width="labelWidth" prop="price">
          <el-input v-model="book.price" placeholder="请输入价格（元）">
            <template #prefix>¥</template>
          </el-input>
        </el-form-item>

        <el-form-item label="出版社" :label-width="labelWidth" prop="publisherId">
          <el-select v-model="book.publisherId" placeholder="请选择出版社" style="width: 100%">
            <el-option v-for="pub in publishers" :key="pub.id" :label="pub.name" :value="String(pub.id)" />
          </el-select>
        </el-form-item>

        <!-- 封面图片上传 -->
        <el-form-item label="封面" :label-width="labelWidth">
          <el-upload v-model:file-list="coverFileList" list-type="picture-card" :auto-upload="false"
            :on-change="handleCoverUploadChange" :on-remove="handleCoverRemove" :on-preview="handleCoverPreview"
            accept="image/jpeg,image/png,image/gif" :limit="1">
            <el-icon>
              <Plus />
            </el-icon>
          </el-upload>
          <div class="upload-tip">最多只能上传 1 张图片</div>

          <!-- 预览上传的封面图片对话框 -->
          <el-dialog v-model="coverPreviewVisible">
            <img w-full :src="coverPreviewUrl" alt="封面预览" />
          </el-dialog>
        </el-form-item>

        <!-- 选择图书所属分类-->
        <el-form-item label="分类" :label-width="labelWidth" prop="category">
          <el-select v-model="book.category" placeholder="请选择分类" style="width: 100%">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="String(cat.id)" />
          </el-select>
        </el-form-item>

        <!-- 状态选择 -->
        <el-form-item label="状态" :label-width="labelWidth">
          <el-select v-model="book.status" placeholder="请选择状态" style="width: 100%">
            <el-option :value="1" label="在库（正常）" />
            <el-option :value="2" label="已下架" />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogFormVisible = false" class="cancel-btn">取消</el-button>
          <el-button type="primary" @click="save" class="save-btn">保存</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.book-management-container {
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
  margin: 0;
  line-height: 1.2;
}

.page-subtitle {
  font-size: 14px;
  color: #909399;
  margin-top: 5px;
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
}

.search-input {
  width: 220px;
}

.date-picker {
  width: 240px;
}

.search-actions {
  margin-left: auto;
}

.search-btn,
.clear-btn {
  border-radius: 4px;
  padding: 10px 20px;
}

.action-area {
  margin-bottom: 20px;
}

.add-btn {
  border-radius: 4px;
  padding: 10px 20px;
  font-weight: 500;
}

.table-card {
  background: #ffffff;
  border-radius: 6px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  margin-bottom: 20px;
}

.data-table {
  border-radius: 6px;
  overflow: hidden;
}

.data-table :deep(.el-table__header) th {
  background-color: #f5f7fa;
  color: #606266;
  font-weight: 600;
}

.price {
  color: #f56c6c;
  font-weight: 500;
}

.action-btn {
  border-radius: 4px;
  padding: 7px 15px;
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

.form-dialog :deep(.el-dialog__header) {
  padding: 20px 20px 10px;
  border-bottom: 1px solid #e8e8e8;
}

.form-dialog :deep(.el-dialog__body) {
  padding: 20px;
}

.form-dialog :deep(.el-form-item) {
  margin-bottom: 18px;
}

.cancel-btn,
.save-btn {
  border-radius: 4px;
  padding: 10px 20px;
}

.cover-uploader {
  width: 120px;
  height: 120px;
  border: 1px dashed #d9d9d9;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  position: relative;
}

.cover-uploader:hover {
  border-color: #409eff;
}

.cover-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  margin-bottom: 8px;
}

.cover-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 4px;
}

.cover-uploader-text {
  font-size: 12px;
  color: #8c939d;
}

.cover-tip {
  margin-top: 8px;
  font-size: 12px;
  color: #67c23a;
}

.book-cover {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
  border: 1px solid #eee;
}

.book-cover-placeholder {
  width: 80px;
  height: 80px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 1px dashed #d9d9d9;
  border-radius: 4px;
  background-color: #f5f7fa;
  color: #909399;
  font-size: 12px;
  cursor: default;
}

.book-cover-placeholder .el-icon {
  font-size: 24px;
  color: #c0c4cc;
}

.upload-tip {
  margin-top: 8px;
  font-size: 12px;
  color: #909399;
  line-height: 1.4;
}
</style>
