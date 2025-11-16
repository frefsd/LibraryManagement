<script setup>
import { onMounted, ref } from 'vue'
import {
  queryPageApi,
  borrowApi,
  returnApi,
  renewApi,
  deleteApi,
  queryAllUsersApi,
  searchBooksApi
} from '@/api/borrow'
import { ElMessage, ElMessageBox } from 'element-plus'

// 搜索表单
const searchForm = ref({
  bookName: '',
  userName: '',
  status: ''
})
// 列表数据
const tableData = ref([])
// 分页配置
const pagination = ref({ currentPage: 1, pageSize: 10, total: 0 })

// 对话框控制
const dialogFormVisible = ref(false)
const formTitle = ref('')
const labelWidth = ref('100px')

// 借阅表单数据
const borrow = ref({
  id: '',
  bookId: '',
  userId: '',
  borrowDate: '',
  dueDate: ''
})

// 用户选项
const userOptions = ref([])
// 图书搜索选项
const bookOptions = ref([])
const bookLoading = ref(false)
// 选中的图书信息
const selectedBook = ref(null)

// 表单引用
const borrowFormRef = ref()

// 表单验证规则
const rules = ref({
  bookId: [
    { required: true, message: '请选择图书', trigger: 'change' }
  ],
  userId: [
    { required: true, message: '请选择借阅人', trigger: 'change' }
  ],
  borrowDate: [
    { required: true, message: '请选择借阅日期', trigger: 'change' }
  ],
  dueDate: [
    { required: true, message: '请选择应还日期', trigger: 'change' }
  ]
})

onMounted(() => {
  queryPage()
  loadUsers()
})

// 加载用户列表
const loadUsers = async () => {
  try {
    const result = await queryAllUsersApi()
    if (result.code) {
      userOptions.value = result.data
    }
  } catch (error) {
    console.error('加载用户列表失败:', error)
    ElMessage.error('加载用户列表失败')
  }
}

// 搜索图书
const searchBooks = async (query) => {
  if (query) {
    bookLoading.value = true
    try {
      const result = await searchBooksApi(query)
      if (result.code) {
        bookOptions.value = result.data
      }
    } catch (error) {
      console.error('搜索图书失败:', error)
      ElMessage.error('搜索图书失败')
    } finally {
      bookLoading.value = false
    }
  } else {
    bookOptions.value = []
  }
}

// 处理图书选择变化
const handleBookChange = (bookId) => {
  const book = bookOptions.value.find(item => item.id === bookId)
  selectedBook.value = book

  // 如果选择了图书，自动设置默认借阅日期和应还日期
  if (book && !borrow.value.borrowDate) {
    const today = new Date()
    borrow.value.borrowDate = today.toISOString().split('T')[0]

    const dueDate = new Date(today)
    dueDate.setDate(dueDate.getDate() + 30) // 默认借阅30天
    borrow.value.dueDate = dueDate.toISOString().split('T')[0]
  }
}

// 分页查询借阅记录
const queryPage = async () => {
  try {
    const result = await queryPageApi(
      searchForm.value.bookName,
      searchForm.value.userName,
      searchForm.value.status,
      pagination.value.currentPage,
      pagination.value.pageSize
    )
    if (result.code) {
      tableData.value = result.data.rows
      pagination.value.total = result.data.total
    } else {
      ElMessage.error(result.msg || '查询失败')
    }
  } catch (error) {
    console.error('查询借阅记录失败:', error)
    ElMessage.error('查询失败')
  }
}

// 清空搜索条件
const clear = () => {
  searchForm.value = { bookName: '', userName: '', status: '' }
  queryPage()
}

// 每页条数变化
const handleSizeChange = (pageSize) => {
  pagination.value.pageSize = pageSize
  queryPage()
}

// 当前页码变化
const handleCurrentChange = (page) => {
  pagination.value.currentPage = page
  queryPage()
}

// 新增借阅
const addBorrow = () => {
  dialogFormVisible.value = true
  formTitle.value = '借阅图书'
  clearBorrowForm()

  // 设置默认日期
  const today = new Date()
  borrow.value.borrowDate = today.toISOString().split('T')[0]

  const dueDate = new Date(today)
  dueDate.setDate(dueDate.getDate() + 30)
  borrow.value.dueDate = dueDate.toISOString().split('T')[0]
}

// 归还图书
const returnBook = async (id) => {
  try {
    await ElMessageBox.confirm('确认归还这本图书吗?', '归还图书', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const result = await returnApi(id)
    if (result.code) {
      ElMessage.success('归还成功')
      queryPage()
    } else {
      ElMessage.error(result.msg || '归还失败')
    }
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('归还失败')
    }
  }
}

// 续借图书
const renewBook = async (id) => {
  try {
    await ElMessageBox.confirm('确认续借这本图书吗?', '续借图书', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const result = await renewApi(id)
    if (result.code) {
      ElMessage.success('续借成功')
      queryPage()
    } else {
      ElMessage.error(result.msg || '续借失败')
    }
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('续借失败')
    }
  }
}

// 清空借阅表单
const clearBorrowForm = () => {
  borrow.value = {
    id: '',
    bookId: '',
    userId: '',
    borrowDate: '',
    dueDate: ''
  }
  selectedBook.value = null
  bookOptions.value = []
}

// 重置表单
const resetForm = (formEl) => {
  if (!formEl) return
  formEl.resetFields()
  clearBorrowForm()
}

// 保存借阅记录
const save = async (formEl) => {
  if (!formEl) return

  await formEl.validate(async (valid) => {
    if (valid) {
      try {
        // 检查是否选择了图书
        if (!selectedBook.value) {
          ElMessage.error('请选择有效的图书')
          return
        }

        // 检查图书状态
        if (selectedBook.value.status !== 1) {
          ElMessage.error('该图书当前不可借阅')
          return
        }

        // 检查借阅日期和应还日期
        const borrowDate = new Date(borrow.value.borrowDate)
        const dueDate = new Date(borrow.value.dueDate)
        if (dueDate <= borrowDate) {
          ElMessage.error('应还日期必须晚于借阅日期')
          return
        }

        const result = await borrowApi(borrow.value)
        if (result.code) {
          ElMessage.success('借阅成功')
          dialogFormVisible.value = false
          queryPage()
        } else {
          ElMessage.error(result.msg || '借阅失败')
        }
      } catch (error) {
        console.error('借阅失败:', error)
        ElMessage.error('借阅失败')
      }
    }
  })
}

// 删除借阅记录
const delById = async (id) => {
  try {
    await ElMessageBox.confirm('您确认删除此借阅记录吗?', '删除借阅记录', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })

    const result = await deleteApi(id)
    if (result.code) {
      ElMessage.success('删除成功')
      queryPage()
    } else {
      ElMessage.error(result.msg || '删除失败')
    }
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  }
}

// 获取状态文本
const getStatusText = (status) => {
  const statusMap = {
    1: '借阅中',
    2: '已归还',
    3: '逾期'
  }
  return statusMap[status] || '未知'
}

// 获取状态标签类型
const getStatusType = (status) => {
  const typeMap = {
    1: 'primary',
    2: 'success',
    3: 'danger'
  }
  return typeMap[status] || 'info'
}

// 禁用借阅日期（不能选择未来的日期）
const disabledBorrowDate = (time) => {
  return time.getTime() > Date.now()
}

// 禁用应还日期（不能早于借阅日期）
const disabledDueDate = (time) => {
  if (!borrow.value.borrowDate) return false
  const borrowDate = new Date(borrow.value.borrowDate)
  return time.getTime() < borrowDate.getTime() - 24 * 60 * 60 * 1000 // 减去一天，确保可以选择借阅日期当天
}
</script>

<template>
  <div>
    <div id="title">借阅管理</div><br>

    <!-- 条件搜索表单 -->
    <el-form :inline="true" :model="searchForm" class="demo-form-inline">
      <el-form-item label="图书名称">
        <el-input v-model="searchForm.bookName" placeholder="请输入图书名称" />
      </el-form-item>
      <el-form-item label="借阅人">
        <el-input v-model="searchForm.userName" placeholder="请输入借阅人姓名" />
      </el-form-item>
      <el-form-item label="状态">
        <el-select v-model="searchForm.status" placeholder="请选择状态">
          <el-option label="全部" value="" />
          <el-option label="借阅中" :value="1" />
          <el-option label="已归还" :value="2" />
          <el-option label="逾期" :value="3" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="queryPage()">查询</el-button>
        <el-button type="info" @click="clear()">清空</el-button>
      </el-form-item>
    </el-form>

    <!-- 功能按钮 -->
    <el-button type="success" @click="addBorrow(); resetForm(borrowFormRef)">+ 借阅图书</el-button>
    <br><br>

    <!-- 列表展示 -->
    <el-table :data="tableData" border style="width: 100%">
      <el-table-column type="index" label="序号" width="55" align="center" />
      <el-table-column prop="bookName" label="图书名称" align="center" />
      <el-table-column prop="userName" label="借阅人" align="center" />
      <el-table-column prop="borrowDate" label="借阅日期" align="center" width="120" />
      <el-table-column prop="dueDate" label="应还日期" align="center" width="120" />
      <el-table-column prop="returnDate" label="归还日期" align="center" width="120" />
      <el-table-column prop="status" label="状态" align="center" width="100">
        <template #default="scope">
          <el-tag :type="getStatusType(scope.row.status)">
            {{ getStatusText(scope.row.status) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="renewCount" label="续借次数" align="center" width="100" />
      <el-table-column label="操作" align="center" width="200">
        <template #default="scope">
          <el-button v-if="scope.row.status === 1" type="warning" size="small" @click="renewBook(scope.row.id)"
            :disabled="scope.row.renewCount >= 3">
            续借
          </el-button>
          <el-button v-if="scope.row.status === 1" type="success" size="small" @click="returnBook(scope.row.id)">
            归还
          </el-button>
          <el-button type="danger" size="small" @click="delById(scope.row.id)">
            删除
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页组件 -->
    <el-pagination v-model:current-page="pagination.currentPage" v-model:page-size="pagination.pageSize"
      :page-sizes="[5, 10, 20, 50]" layout="total, sizes, prev, pager, next, jumper" :total="pagination.total"
      @size-change="handleSizeChange" @current-change="handleCurrentChange" />

    <!-- 借阅图书对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="40%">
      <el-form :model="borrow" ref="borrowFormRef" :rules="rules">
        <el-form-item label="选择图书" :label-width="labelWidth" prop="bookId">
          <el-select v-model="borrow.bookId" filterable remote reserve-keyword placeholder="请输入图书名称搜索"
            :remote-method="searchBooks" :loading="bookLoading" style="width: 100%" @change="handleBookChange">
            <el-option v-for="item in bookOptions" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="图书信息" :label-width="labelWidth" v-if="selectedBook">
          <div style="text-align: left; padding: 8px; background: #f5f7fa; border-radius: 4px;">
            <p><strong>书名：</strong>{{ selectedBook.name }}</p>
            <p><strong>作者：</strong>{{ selectedBook.author }}</p>
            <p><strong>出版社：</strong>{{ selectedBook.publisherName }}</p>
            <p><strong>当前状态：</strong>
              <el-tag :type="selectedBook.status === 1 ? 'success' : 'danger'">
                {{ selectedBook.status === 1 ? '在库' : '已借出' }}
              </el-tag>
            </p>
          </div>
        </el-form-item>

        <el-form-item label="借阅人" :label-width="labelWidth" prop="userId">
          <el-select v-model="borrow.userId" placeholder="请选择借阅人" style="width: 100%;">
            <el-option v-for="user in userOptions" :key="user.id" :label="user.name" :value="user.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="借阅日期" :label-width="labelWidth" prop="borrowDate">
          <el-date-picker v-model="borrow.borrowDate" type="date" placeholder="请选择借阅日期" value-format="YYYY-MM-DD"
            style="width: 100%;" :disabled-date="disabledBorrowDate" />
        </el-form-item>

        <el-form-item label="应还日期" :label-width="labelWidth" prop="dueDate">
          <el-date-picker v-model="borrow.dueDate" type="date" placeholder="请选择应还日期" value-format="YYYY-MM-DD"
            style="width: 100%;" :disabled-date="disabledDueDate" />
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogFormVisible = false; resetForm(borrowFormRef)">取消</el-button>
          <el-button type="primary" @click="save(borrowFormRef)">保存</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>



<style scoped>
#title {
  font-size: 20px;
  font-weight: 600;
}
</style>
