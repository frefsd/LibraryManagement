<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { queryPageApi, borrowApi, returnApi, renewApi } from '@/api/borrow'
import { getAvailableBooksApi } from '@/api/book'
import { ElMessage, ElMessageBox } from 'element-plus'

// ====== 数据 ======
const loading = ref(false)
const borrowLoading = ref(false)
const tableData = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)
const booksLoading = ref(false)
const bookOptions = ref([])

const showBorrowDialog = ref(false)
const borrowForm = ref({
  bookId: null,
  userInput: ''
})
const borrowFormRef = ref()

const borrowRules = {
  bookId: [{ required: true, message: '请选择图书', trigger: 'change' }],
  userInput: [
    {
      required: true,
      message: '请输入用户ID或用户名',
      trigger: 'blur'
    },
    {
      validator: (rule, value, callback) => {
        if (!value || !value.toString().trim()) {
          callback(new Error('不能为空'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

// ====== 方法 ======
const loadBooks = async (keyword) => {
  const kw = typeof keyword === 'string' ? keyword.trim() : ''
  booksLoading.value = true
  try {
    const res = await getAvailableBooksApi({ keyword: kw, page: 1, pageSize: 1000 })
    if (res && Array.isArray(res.rows)) {
      bookOptions.value = res.rows
    } else {
      bookOptions.value = []
    }
  } catch (err) {
    console.error('加载图书列表失败:', err)
    bookOptions.value = []
    ElMessage.error('加载图书列表失败')
  } finally {
    booksLoading.value = false
  }
}

const handleBookSelectFocus = () => {
  if (bookOptions.value.length === 0) {
    loadBooks('')
  }
}

const handleDialogClosed = () => {
  borrowForm.value.bookId = null,
    borrowForm.value.userInput = ''
  bookOptions.value = []
  nextTick(() => {
    borrowFormRef.value?.clearValidate()
  })
}

const loadTableData = async () => {
  loading.value = true
  try {
    const res = await queryPageApi(page.value, pageSize.value)
    if (res.data?.rows) {
      tableData.value = res.data.rows
      total.value = res.data.total || 0
    } else {
      tableData.value = []
      total.value = 0
    }
  } catch (err) {
    ElMessage.error('加载借阅记录失败')
  } finally {
    loading.value = false
  }
}

const handleSizeChange = (val) => {
  pageSize.value = val
  page.value = 1
  loadTableData()
}

const handleCurrentChange = (val) => {
  page.value = val
  loadTableData()
}

const handleReturn = async (id) => {
  try {
    await ElMessageBox.confirm('确认归还此图书？', '提示', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'info'
    })
    await returnApi(id)
    ElMessage.success('归还成功')
    loadTableData()
  } catch (err) {
    if (err !== 'cancel') {
      ElMessage.error('归还失败，请重试')
    }
  }
}

const handleRenew = async (id) => {
  try {
    await ElMessageBox.confirm("确认为该图书办理续借？续借将延长应还日期。", '续借提示', {
      confirmButtonText: '确认续借',
      cancelButtonClass: '取消',
      type: 'warning'
    })

    await renewApi(id) //调用续借接口
    ElMessage.success('续借成功')
    loadTableData() //刷新列表
  } catch (err) {
    if (err !== 'cancel') {
      console.error('续借失败:', err)
    }
  }
}


const handleBorrowConfirm = () => {
  borrowFormRef.value?.validate(async (valid) => {
    if (!valid) return
    borrowLoading.value = true
    try {
      // 传 { bookId, userInput } 给后端
      await borrowApi({
        bookId: borrowForm.value.bookId,
        userInput: borrowForm.value.userInput.trim()
      })
      ElMessage.success('借阅成功')
      showBorrowDialog.value = false
      loadTableData()
    } catch (err) {
      let msg = '借阅失败，请检查输入信息'
      const errorData = err.response?.data
      if (errorData) {
        msg = errorData.message || errorData.msg || msg
        if (msg.includes('不存在')) msg = '图书或用户不存在，请检查输入'
        else if (msg.includes('下架')) msg = '该图书已下架，无法借阅'
        else if (msg.includes('借出') || msg.includes('已被借')) msg = '该图书已被借出，无法重复借阅'
        else if (msg.includes('库存')) msg = '该图书库存不足，无法借阅'
      }
    } finally {
      borrowLoading.value = false
    }
  })
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')} ${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

const formatSimpleDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '无效时间'
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

const getStatusText = (status) => {
  return status === 1 ? '借阅中' : status === 2 ? '已归还' : '逾期'
}

onMounted(() => {
  loadTableData()
})
</script>


<template>
  <div class="borrow-container">
    <!-- 标题与操作 -->
    <div class="header-section">
      <h2 class="page-title">借阅管理</h2>
      <el-button type="primary" @click="showBorrowDialog = true" icon="Plus" class="add-button">新增借阅</el-button>
    </div>

    <!-- 借阅表格 -->
    <el-card shadow="never" style="margin-top: 16px;">
      <el-table :data="tableData" border stripe v-loading="loading" style="width: 100%" max-height="600">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="bookName" label="图书名称" min-width="180" />
        <el-table-column prop="userName" label="借阅人" min-width="100" />
        <el-table-column label="借出日期" min-width="140" align="center">
          <template #default="{ row }">{{ formatDate(row.borrowDate) }}</template>
        </el-table-column>
        <el-table-column label="应还日期" min-width="140" align="center">
          <template #default="{ row }">{{ formatDate(row.dueDate) }}</template>
        </el-table-column>
        <el-table-column label="实际归还" min-width="140" align="center">
          <template #default="{ row }">
            {{ row.actualReturnDate ? formatDate(row.actualReturnDate) : '未归还' }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'warning' :
              row.status === 2 ? 'success' :
                'danger'
              " effect="plain">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" align="center">
          <template #default="{ row }">
            <!--归还按钮（仅未归还时显示）-->
            <el-button v-if="!row.actualReturnDate" size="default" type="primary" @click="handleReturn(row.id)">
              归还
            </el-button>
            <!--续借按钮（仅未归还和 "借阅中" 时显示）-->
            <el-button v-if="!row.actualReturnDate && row.status == 1" size="default" type="warning"
              @click="handleRenew(row.id)">
              续借
            </el-button>
            <span v-else-if="row.actualReturnDate">—</span>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination v-if="total > 0" @size-change="handleSizeChange" @current-change="handleCurrentChange"
          :current-page="page" :page-sizes="[10, 20, 50]" :page-size="pageSize"
          layout="total, sizes, prev, pager, next, jumper" :total="total" is-background class="pagination" background />
      </div>
    </el-card>

    <!-- 新增借阅弹窗 -->
    <el-dialog v-model="showBorrowDialog" title="新增借阅" width="500px" destroy-on-close @closed="handleDialogClosed"
      class="borrow-dialog">
      <el-form :model="borrowForm" :rules="borrowRules" ref="borrowFormRef" label-position="left" label-width="90px"
        class="borrow-form">
        <el-form-item label="图书名称" prop="bookId" class="form-item-enhanced">
          <el-select v-model="borrowForm.bookId" filterable remote reserve-keyword placeholder="请输入书名或作者进行搜索"
            :remote-method="loadBooks" :loading="booksLoading" :clearable="true" style="width: 100%"
            @focus="handleBookSelectFocus" class="enhanced-select">
            <el-option v-for="book in bookOptions" :key="book.id" :value="book.id"
              :label="`${book.name} - ${book.author}`">
              <div class="book-option-item">
                <div class="book-title">{{ book.name }}</div>
                <div class="book-meta">
                  <span class="book-author">作者：{{ book.author }}</span>
                  <el-tag size="small" :type="(book.totalCopies - book.borrowedCopies) > 0 ? 'success' : 'danger'"
                    effect="plain" class="book-stock">
                    库存：{{ book.totalCopies - book.borrowedCopies }}/{{ book.totalCopies }}
                  </el-tag>
                </div>
                <div class="book-details">
                  <span class="book-category">分类：{{ book.category?.name || book.categoryName || '未知' }}</span>
                  <span v-if="book.publisher" class="book-publisher">
                    出版社：{{ book.publisher.name || book.publisher }}
                  </span>
                  <span v-if="book.publishDate" class="book-publish-date">
                    出版日期：{{ formatSimpleDate(book.publishDate) }}
                  </span>
                </div>
              </div>
            </el-option>
            <template #empty>
              <div style="padding: 8px; text-align: center; color: #909399;">
                {{ booksLoading ? '搜索中...' : '暂无数据' }}
              </div>
            </template>
          </el-select>
        </el-form-item>
        <!-- 修改：用户输入框改为普通文本输入 -->
        <el-form-item label="用户名称" prop="userInput" class="form-item-enhanced">
          <el-input v-model="borrowForm.userInput" placeholder="请输入用户ID或用户名" clearable style="width: 100%"
            class="enhanced-input" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showBorrowDialog = false" class="cancel-btn">取消</el-button>
          <el-button type="primary" @click="handleBorrowConfirm" :loading="borrowLoading"
            class="confirm-btn">确定</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>



<style scoped>
.borrow-container {
  padding: 20px;
  background-color: #f9fafa;
  min-height: 100vh;
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #333;
  margin: 0;
}

.add-button {
  border-radius: 6px;
  padding: 10px 16px;
  font-weight: 500;
  box-shadow: 0 2px 4px rgba(64, 158, 255, 0.2);
  transition: all 0.3s ease;
}

.add-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(64, 158, 255, 0.3);
}

.el-table .el-table__header th {
  background-color: #f8f9fa;
  font-weight: 600;
}

.el-table .el-table__body td {
  padding: 10px 0;
}

.el-pagination {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

:deep(.el-select-dropdown__item) {
  min-height: 80px !important;
  padding: 8px 12px !important;
  overflow: visible !important;
  line-height: 1.4;
  display: flex !important;
  align-items: flex-start !important;
}

/* 自定义图书选项样式 */
.book-option-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
  font-size: 13px;
}

.book-title {
  font-weight: 500;
  color: #303133;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.book-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #606266;
}

.book-author {
  color: #8492a6;
}

.book-stock {
  flex-shrink: 0;
}

.book-details {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  font-size: 11px;
  color: #909399;
}

.book-category,
.book-publisher,
.book-publish-date {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 新增借阅对话框优化样式 */
:deep(.borrow-dialog) {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

:deep(.borrow-dialog .el-dialog__header) {
  padding: 20px 24px 10px;
  margin-right: 0;
  background: linear-gradient(135deg, #f5f7fa 0%, #ffffff 100%);
  border-bottom: 1px solid #eef0f3;
}

:deep(.borrow-dialog .el-dialog__title) {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  letter-spacing: 0.5px;
}

:deep(.borrow-dialog .el-dialog__body) {
  padding: 24px;
}

.borrow-form {
  padding: 0 8px;
}

.form-item-enhanced {
  margin-bottom: 24px;
}

:deep(.form-item-enhanced .el-form-item__label) {
  font-weight: 600;
  color: #34495e;
  padding-right: 16px;
  font-size: 14px;
}

.enhanced-select:deep(.el-input__inner) {
  border-radius: 8px;
  border: 1px solid #dcdfe6;
  padding: 10px 12px;
  height: 40px;
  transition: all 0.3s ease;
  font-size: 14px;
}

.enhanced-select:deep(.el-input__inner:focus) {
  border-color: #409eff;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.1);
}

.enhanced-input:deep(.el-input__inner) {
  border-radius: 8px;
  border: 1px solid #dcdfe6;
  padding: 10px 12px;
  height: 40px;
  transition: all 0.3s ease;
  font-size: 14px;
}

.enhanced-input:deep(.el-input__inner:focus) {
  border-color: #409eff;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.1);
}

:deep(.borrow-dialog .el-dialog__footer) {
  padding: 16px 24px 20px;
  border-top: 1px solid #eef0f3;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.cancel-btn {
  border-radius: 6px;
  padding: 10px 20px;
  border: 1px solid #dcdfe6;
  transition: all 0.3s ease;
}

.cancel-btn:hover {
  border-color: #c0c4cc;
  background-color: #f5f7fa;
  transform: translateY(-1px);
}

.confirm-btn {
  border-radius: 6px;
  padding: 10px 20px;
  font-weight: 500;
  background: linear-gradient(135deg, #409eff 0%, #337ecc 100%);
  border: none;
  box-shadow: 0 2px 4px rgba(64, 158, 255, 0.3);
  transition: all 0.3s ease;
}

.confirm-btn:hover {
  background: linear-gradient(135deg, #66b1ff 0%, #409eff 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(64, 158, 255, 0.4);
}

.confirm-btn:active {
  transform: translateY(0);
  box-shadow: 0 2px 4px rgba(64, 158, 255, 0.3);
}
</style>