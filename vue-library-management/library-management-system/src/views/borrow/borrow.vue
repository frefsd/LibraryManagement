<!-- src/views/Borrow.vue -->
<template>
  <div class="borrow-container">
    <el-card class="box-card">
      <div slot="header" class="clearfix">
        <span>借阅管理</span>
        <el-button style="float: right; padding: 3px 0" type="primary" size="small" @click="showBorrowDialog = true">
          新增借阅
        </el-button>
      </div>

      <!-- 借阅表格 -->
      <el-table :data="tableData" border style="width: 100%" v-loading="loading">
        <el-table-column type="index" label="序号" width="60"></el-table-column>
        <el-table-column prop="bookName" label="图书名称" width="200"></el-table-column>
        <el-table-column prop="userName" label="借阅人" width="120"></el-table-column>
        <el-table-column prop="borrowDate" label="借出日期" width="160">
          <template #default="{ row }">
            {{ formatDate(row.borrowDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="dueDate" label="应还日期" width="160">
          <template #default="{ row }">
            {{ formatDate(row.dueDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="actualReturnDate" label="实际归还" width="160">
          <template #default="{ row }">
            {{ row.actualReturnDate ? formatDate(row.actualReturnDate) : '未归还' }}
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.status === 1" type="warning">借阅中</el-tag>
            <el-tag v-else-if="row.status === 2" type="success">已归还</el-tag>
            <el-tag v-else-if="row.status === 3" type="danger">逾期</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button
              v-if="!row.actualReturnDate"
              size="mini"
              type="primary"
              @click="handleReturn(row.id)"
            >
              归还
            </el-button>
            <span v-else>—</span>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        :current-page="page"
        :page-sizes="[10, 20, 50]"
        :page-size="pageSize"
        layout="total, sizes, prev, pager, next, jumper"
        :total="total"
        style="margin-top: 15px; text-align: right"
      />

      <!-- 新增借阅弹窗 -->
      <el-dialog title="新增借阅" v-model="showBorrowDialog" width="400px">
        <el-form :model="borrowForm" label-width="80px" :rules="borrowRules" ref="borrowFormRef">
          <el-form-item label="图书ID" prop="bookId">
            <el-input-number v-model="borrowForm.bookId" :min="1" controls-position="right" style="width: 100%" />
          </el-form-item>
          <el-form-item label="用户ID" prop="userId">
            <el-input-number v-model="borrowForm.userId" :min="1" controls-position="right" style="width: 100%" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="showBorrowDialog = false">取消</el-button>
          <el-button type="primary" @click="handleBorrowConfirm" :loading="borrowLoading">确定</el-button>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script>
import { queryPageApi, borrowApi, returnApi } from '@/api/borrow'
import { ElMessage } from 'element-plus'

export default {
  name: 'Borrow',
  data() {
    return {
      loading: false,
      borrowLoading: false,
      tableData: [],
      total: 0,
      page: 1,
      pageSize: 10,

      showBorrowDialog: false,
      borrowForm: {
        bookId: null,
        userId: null
      },
      borrowRules: {
        bookId: [{ required: true, message: '请输入图书ID', trigger: 'blur' }],
        userId: [{ required: true, message: '请输入用户ID', trigger: 'blur' }]
      }
    }
  },
  created() {
    this.loadTableData()
  },
  methods: {
    async loadTableData() {
      this.loading = true
      try {
        const res = await queryPageApi(this.page, this.pageSize)
        this.tableData = res.data?.rows || []
        this.total = res.data?.total || 0
      } catch (err) {
        console.error('加载失败:', err)
        ElMessage.error('加载借阅记录失败')
      } finally {
        this.loading = false
      }
    },

    handleSizeChange(val) {
      this.pageSize = val
      this.page = 1
      this.loadTableData()
    },

    handleCurrentChange(val) {
      this.page = val
      this.loadTableData()
    },

    async handleReturn(id) {
      try {
        await returnApi(id)
        ElMessage.success('归还成功')
        this.loadTableData()
      } catch (err) {
        console.error('归还失败:', err)
        let msg = '归还失败，请重试'
        if (err.response?.data) {
          msg = err.response.data.message || err.response.data.msg || msg
        }
        ElMessage.error(msg)
      }
    },

    handleBorrowConfirm() {
      this.$refs.borrowFormRef.validate(async (valid) => {
        if (!valid) return

        this.borrowLoading = true
        try {
          await borrowApi(this.borrowForm)
          ElMessage.success('借阅成功')
          this.showBorrowDialog = false
          this.borrowForm = { bookId: null, userId: null }
          this.loadTableData()
        } catch (err) {
          console.error('借阅失败:', err)

          // 关键：解析后端返回的错误信息
          let msg = '借阅失败，请检查输入信息'
          const errorData = err.response?.data

          if (errorData) {
            // 优先使用后端返回的 message 或 msg 字段
            msg = errorData.message || errorData.msg || msg

            // 可选：根据特定关键词进一步细化提示（如果后端不返回结构化 code）
            if (msg.includes('不存在')) {
              msg = '图书或用户不存在，请检查ID'
            } else if (msg.includes('下架')) {
              msg = '该图书已下架，无法借阅'
            } else if (msg.includes('借出') || msg.includes('已被借')) {
              msg = '该图书已被借出，无法重复借阅'
            }
          }

          ElMessage.error(msg)
        } finally {
          this.borrowLoading = false
        }
      })
    },

    formatDate(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      if (isNaN(date.getTime())) return '无效时间'
      return (
        date.getFullYear() +
        '-' +
        String(date.getMonth() + 1).padStart(2, '0') +
        '-' +
        String(date.getDate()).padStart(2, '0') +
        ' ' +
        String(date.getHours()).padStart(2, '0') +
        ':' +
        String(date.getMinutes()).padStart(2, '0')
      )
    }
  }
}
</script>

<style scoped>
.borrow-container {
  padding: 20px;
}
</style>