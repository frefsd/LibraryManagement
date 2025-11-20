<script setup>
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
  status: 1 // 1=在库，2=已下架
})

const bookFormRef = ref()

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

// ============ 加载分类 ============
const loadCategories = async () => {
  const res = await queryAllCategoryApi()
  if (res.code) {
    categories.value = res.data
  }
}

// ============ 加载出版社 ============
const loadPublishers = async () => {
  const res = await queryAllPublisherApi()
  if (res.code) {
    publishers.value = res.data
  }
}

// ============ 分页与搜索 ============
const queryPage = async () => {
  const result = await queryPageApi(
    searchBook.value.begin,
    searchBook.value.end,
    searchBook.value.name,
    pagination.value.currentPage,
    pagination.value.pageSize
  )

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
    status: 1
  }
}

const addBook = () => {
  dialogFormVisible.value = true
  formTitle.value = '新增图书'
  clearBook()
  resetForm()
}

const updateBook = async (id) => {
  clearBook()
  dialogFormVisible.value = true
  formTitle.value = '编辑图书'

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
  }
}

const resetForm = () => {
  if (bookFormRef.value) {
    bookFormRef.value.resetFields()
  }
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

    const payload = {
      name: book.value.name,
      author: book.value.author,
      publishDate: book.value.publishDate,
      price: parseFloat(book.value.price) || 0,
      categoryId,
      publisherId,
      status: parseInt(book.value.status) || 1
    }

    const bookId = parseInt(book.value.id)
    if (!isNaN(bookId) && bookId > 0) {
      payload.id = bookId
    }

    const api = bookId > 0 ? updateApi(payload) : addApi(payload)
    const result = await api

    if (result.code) {
      ElMessage.success('操作成功')
      dialogFormVisible.value = false
      queryPage()
    } else {
      ElMessage.error(result.msg || '操作失败')
    }
  } catch (error) {
    console.error('保存失败:', error)
    ElMessage.warning('请检查表单内容')
  }
}

const delById = async (id) => {
  try {
    await ElMessageBox.confirm('确认删除此图书？', '提示', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const result = await deleteApi(id)
    // 成功响应（HTTP 2xx）
    if (result?.code) {
      ElMessage.success('删除成功')
      queryPage()
    } else {
      ElMessage.error(result?.msg || '删除失败')
    }
  } catch (error) {
    // 用户取消
    if (error === 'cancel') return

    if (error.response?.data) {
      const msg = error.response.data.msg || '删除失败'
      ElMessage.error(msg)
    } else {
      ElMessage.error('网络错误，请稍后重试')
    }
  }
}

// ============ 日期格式化 ============
const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'

  const pad = (num) => String(num).padStart(2, '0')
  const year = date.getFullYear()
  const month = pad(date.getMonth() + 1)
  const day = pad(date.getDate())

  return `${year}-${month}-${day}`
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'

  const pad = (num) => String(num).padStart(2, '0')
  const year = date.getFullYear()
  const month = pad(date.getMonth() + 1)
  const day = pad(date.getDate())
  const hours = pad(date.getHours())
  const minutes = pad(date.getMinutes())
  const seconds = pad(date.getSeconds())

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
}
</script>

<template>
  <div>
    <!-- 标题 -->
    <div id="title">图书管理</div><br />

    <!-- 搜索表单 -->
    <el-form :inline="true" :model="searchBook" class="demo-form-inline">
      <el-form-item label="图书名称">
        <el-input v-model="searchBook.name" placeholder="请输入图书名称" />
      </el-form-item>

      <el-form-item label="出版日期">
        <el-date-picker v-model="searchBook.date" type="daterange" range-separator=" 至 " start-placeholder="开始时间"
          end-placeholder="结束时间" value-format="YYYY-MM-DD" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="queryPage">查询</el-button>
        <el-button type="info" @click="clear">清空</el-button>
      </el-form-item>
    </el-form>

    <!-- 新增按钮 -->
    <el-button type="success" @click="addBook">+ 新增图书</el-button>
    <br /><br />

    <!-- 图书列表 -->
    <el-table :data="tableData" border style="width: 100%" fit>
      <el-table-column label="序号" width="55" align="center">
        <template #default="scope">
          {{ scope.row.id }}
        </template>
      </el-table-column>
      <el-table-column prop="name" label="图书名称" align="center" width="200" />
      <el-table-column prop="author" label="作者" align="center" width="100" />
      <el-table-column label="分类" align="center" width="100">
        <template #default="scope">
          {{ scope.row.category?.name || '未知' }}
        </template>
      </el-table-column>
      <el-table-column label="出版日期" align="center" width="150">
        <template #default="scope">
          {{ formatDate(scope.row.publishDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="price" label="价格" align="center" width="100" />
      <el-table-column label="状态" align="center" width="100">
        <template #default="scope">
          {{ getStatusText(scope.row.status) }}
        </template>
      </el-table-column>
      <el-table-column label="最后修改时间" align="center" width="180">
        <template #default="scope">
          {{ formatDateTime(scope.row.updateTime) }}
        </template>
      </el-table-column>
      <el-table-column label="操作" align="center" width="160">
        <template #default="scope">
          <el-button type="primary" size="small" @click="updateBook(scope.row.id)">编辑</el-button>
          <el-button type="danger" size="small" @click="delById(scope.row.id)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>
    <br />

    <!-- 分页 -->
    <el-pagination v-model:current-page="pagination.currentPage" v-model:page-size="pagination.pageSize"
      :page-sizes="[5, 10, 20, 50, 100]" layout="total, sizes, prev, pager, next, jumper" :total="pagination.total"
      @size-change="handleSizeChange" @current-change="handleCurrentChange" />

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="400px">
      <el-form :model="book" ref="bookFormRef" :rules="rules" label-position="left">
        <el-form-item label="图书名称" :label-width="labelWidth" prop="name">
          <el-input v-model="book.name" placeholder="请输入图书名称" />
        </el-form-item>

        <el-form-item label="作者" :label-width="labelWidth" prop="author">
          <el-input v-model="book.author" placeholder="请输入作者" />
        </el-form-item>

        <el-form-item label="出版日期" :label-width="labelWidth" prop="publishDate">
          <el-date-picker v-model="book.publishDate" type="date" placeholder="请选择出版日期" value-format="YYYY-MM-DD"
            style="width: 100%" />
        </el-form-item>

        <el-form-item label="价格" :label-width="labelWidth" prop="price">
          <el-input v-model="book.price" placeholder="请输入价格（元）" />
        </el-form-item>

        <el-form-item label="出版社" :label-width="labelWidth" prop="publisherId">
          <el-select v-model="book.publisherId" placeholder="请选择出版社" style="width: 100%">
            <el-option v-for="pub in publishers" :key="pub.id" :label="pub.name" :value="String(pub.id)" />
          </el-select>
        </el-form-item>

        <el-form-item label="分类" :label-width="labelWidth" prop="category">
          <el-select v-model="book.category" placeholder="请选择分类" style="width: 100%">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="String(cat.id)" />
          </el-select>
        </el-form-item>

        <!-- 新增：状态选择 -->
        <el-form-item label="状态" :label-width="labelWidth">
          <el-select v-model="book.status" placeholder="请选择状态" style="width: 100%">
            <el-option :value="1" label="在库（正常）" />
            <el-option :value="2" label="已下架" />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogFormVisible = false">取消</el-button>
          <el-button type="primary" @click="save">保存</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
#title {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 16px;
}
</style>