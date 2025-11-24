<template>
  <div class="borrow-container">
    <!-- 标题与操作 -->
    <div class="header-section">
      <h2 class="page-title">借阅管理</h2>
      <el-button type="primary" @click="showBorrowDialog = true" icon="Plus">新增借阅</el-button>
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
        <el-table-column label="操作" width="100" align="center">
          <template #default="{ row }">
            <el-button v-if="!row.actualReturnDate" size="small" type="primary" @click="handleReturn(row.id)">
              归还
            </el-button>
            <span v-else>—</span>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination v-if="total > 0" @size-change="handleSizeChange" @current-change="handleCurrentChange"
        :current-page="page" :page-sizes="[10, 20, 50]" :page-size="pageSize"
        layout="total, sizes, prev, pager, next, jumper" :total="total"
        style="margin-top: 20px; justify-content: center; display: flex;" />
    </el-card>

    <!-- 新增借阅弹窗 -->
    <el-dialog v-model="showBorrowDialog" title="新增借阅" width="500px" destroy-on-close @closed="handleDialogClosed">
      <el-form :model="borrowForm" :rules="borrowRules" ref="borrowFormRef" label-position="left" label-width="90px">
        <el-form-item label="图书" prop="bookId">
          <el-select v-model="borrowForm.bookId" filterable remote reserve-keyword placeholder="请输入书名或作者进行搜索"
            :remote-method="loadBooks" :loading="booksLoading" :clearable="true" style="width: 100%"
            @focus="handleBookSelectFocus">
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
        <el-form-item label="用户ID" prop="userId">
          <el-input-number v-model.number="borrowForm.userId" :min="1" controls-position="right" style="width: 100%"
            placeholder="请输入用户ID" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showBorrowDialog = false">取消</el-button>
        <el-button type="primary" @click="handleBorrowConfirm" :loading="borrowLoading">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { queryPageApi, borrowApi, returnApi } from '@/api/borrow'
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
  userId: null
})
const borrowFormRef = ref()

const borrowRules = {
  bookId: [{ required: true, message: '请选择图书', trigger: 'change' }],
  userId: [{ required: true, message: '请输入用户ID', trigger: 'blur' }]
}

// ====== 方法 ======
const loadBooks = async (keyword) => {
  const kw = typeof keyword === 'string' ? keyword.trim() : ''
  booksLoading.value = true
  try {
    const res = await getAvailableBooksApi({ keyword: kw, page: 1, pageSize: 20 })
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
  borrowForm.value = { bookId: null, userId: null }
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

const handleBorrowConfirm = () => {
  borrowFormRef.value?.validate(async (valid) => {
    if (!valid) return
    borrowLoading.value = true
    try {
      await borrowApi(borrowForm.value)
      ElMessage.success('借阅成功')
      showBorrowDialog.value = false
      loadTableData()
    } catch (err) {
      let msg = '借阅失败，请检查输入信息'
      const errorData = err.response?.data
      if (errorData) {
        msg = errorData.message || errorData.msg || msg
        if (msg.includes('不存在')) msg = '图书或用户不存在，请检查ID'
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
</style>