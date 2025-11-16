<script setup>
import { onMounted, ref } from 'vue'
import { queryAllApi} from '@/api/category'
import { ElMessage, ElMessageBox } from 'element-plus'

// 搜索表单
const searchForm = ref({ name: '' })
// 列表数据
const tableData = ref([])
// 分页配置
const pagination = ref({ currentPage: 1, pageSize: 10, total: 0 })

// 对话框控制
const dialogFormVisible = ref(false)
const formTitle = ref('')
const labelWidth = ref('80px')

// 分类表单数据
const category = ref({
  id: '',
  name: '',
  parentId: 0,
  sort: 0,
  status: 1
})

// 父分类列表（用于下拉选择）
const parentCategories = ref([])

// 表单引用
const categoryFormRef = ref()

// 表单验证规则
const rules = ref({
  name: [
    { required: true, message: '分类名称为必填项', trigger: 'blur' },
    { min: 1, max: 50, message: '分类名称长度为1-50个字', trigger: 'blur' }
  ],
  sort: [
    { required: true, message: '排序为必填项', trigger: 'blur' }
  ]
})

onMounted(() => {
  queryPage()
  loadParentCategories()
})

// 加载父分类列表
const loadParentCategories = async () => {
  try {
    const result = await queryAllApi()
    if (result.code) {
      parentCategories.value = result.data
    }
  } catch (error) {
    console.error('加载父分类失败:', error)
  }
}

// 分页查询
const queryPage = async () => {
  try {
    const result = await queryPageApi(
      searchForm.value.name,
      pagination.value.currentPage,
      pagination.value.pageSize
    )
    if (result.code) {
      tableData.value = result.data.rows
      pagination.value.total = result.data.total
    }
  } catch (error) {
    console.error('查询分类列表失败:', error)
    ElMessage.error('查询失败')
  }
}

// 清空搜索
const clear = () => {
  searchForm.value = { name: '' }
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

// 新增分类
const addCategory = () => {
  dialogFormVisible.value = true
  formTitle.value = '新增分类'
  clearCategoryForm()
}

// 修改分类
const updateCategory = async (id) => {
  try {
    // 这里需要调用查询单个分类的API
    // const result = await queryInfoApi(id)
    // if (result.code) {
    //   category.value = result.data
    //   dialogFormVisible.value = true
    //   formTitle.value = '修改分类'
    // }
    
    // 临时实现：从列表数据中查找
    const found = tableData.value.find(item => item.id === id)
    if (found) {
      category.value = { ...found }
      dialogFormVisible.value = true
      formTitle.value = '修改分类'
    }
  } catch (error) {
    console.error('加载分类详情失败:', error)
    ElMessage.error('加载失败')
  }
}

// 清空表单
const clearCategoryForm = () => {
  category.value = {
    id: '',
    name: '',
    parentId: 0,
    sort: 0,
    status: 1
  }
}

// 重置表单
const resetForm = (formEl) => {
  if (!formEl) return
  formEl.resetFields()
}

// 保存分类
const save = async (formEl) => {
  if (!formEl) return
  
  await formEl.validate(async (valid) => {
    if (valid) {
      try {
        let result
        if (category.value.id) {
          result = await updateApi(category.value)
        } else {
          result = await addApi(category.value)
        }

        if (result.code) {
          ElMessage.success('操作成功')
          dialogFormVisible.value = false
          queryPage()
          loadParentCategories() // 刷新父分类列表
        } else {
          ElMessage.error(result.msg)
        }
      } catch (error) {
        console.error('保存分类失败:', error)
        ElMessage.error('保存失败')
      }
    }
  })
}

// 删除分类
const delById = async (id) => {
  try {
    await ElMessageBox.confirm('您确认删除此分类吗?', '删除分类', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning'
    })
    
    const result = await deleteApi(id)
    if (result.code) {
      ElMessage.success('删除成功')
      queryPage()
      loadParentCategories() // 刷新父分类列表
    } else {
      ElMessage.error(result.msg)
    }
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
    }
  }
}
</script>

<template>
  <div>
    <div id="title">分类管理</div><br>
    
    <!-- 条件搜索表单 -->
    <el-form :inline="true" :model="searchForm" class="demo-form-inline">
      <el-form-item label="分类名称">
        <el-input v-model="searchForm.name" placeholder="请输入分类名称"/>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="queryPage()">查询</el-button>
        <el-button type="info" @click="clear()">清空</el-button>
      </el-form-item>
    </el-form>
    
    <!-- 功能按钮 -->
    <el-button type="success" @click="addCategory();resetForm(categoryFormRef)">+ 新增分类</el-button>
    <br><br>
    
    <!-- 列表展示 -->
    <el-table :data="tableData" border style="width: 100%">
      <el-table-column type="index" label="序号" width="55" align="center"/>
      <el-table-column prop="name" label="分类名称" align="center"/>
      <el-table-column prop="parentName" label="父分类" align="center"/>
      <el-table-column prop="sort" label="排序" align="center" width="100"/>
      <el-table-column prop="status" label="状态" align="center" width="100">
        <template #default="scope">
          <el-tag :type="scope.row.status === 1 ? 'success' : 'danger'">
            {{ scope.row.status === 1 ? '启用' : '禁用' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" align="center">
        <template #default="scope">
          <el-button type="primary" size="small" @click="updateCategory(scope.row.id);resetForm(categoryFormRef)">编辑</el-button>
          <el-button type="danger" size="small" @click="delById(scope.row.id)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>
    
    <!-- 分页组件 -->
    <el-pagination
      v-model:current-page="pagination.currentPage"
      v-model:page-size="pagination.pageSize"
      :page-sizes="[5, 10, 20, 50]"
      layout="total, sizes, prev, pager, next, jumper"
      :total="pagination.total"
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
    />

    <!-- 新增/修改分类对话框 -->
    <el-dialog v-model="dialogFormVisible" :title="formTitle" width="35%">
      <el-form :model="category" ref="categoryFormRef" :rules="rules">
        <el-form-item label="分类名称" :label-width="labelWidth" prop="name">
          <el-input v-model="category.name" placeholder="请输入分类名称" />
        </el-form-item>

        <el-form-item label="父分类" :label-width="labelWidth" prop="parentId">
          <el-select v-model="category.parentId" placeholder="请选择父分类" style="width: 100%;">
            <el-option label="顶级分类" :value="0" />
            <el-option v-for="cat in parentCategories" :label="cat.name" :value="cat.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="排序" :label-width="labelWidth" prop="sort">
          <el-input-number v-model="category.sort" :min="0" :max="999" />
        </el-form-item>

        <el-form-item label="状态" :label-width="labelWidth" prop="status">
          <el-radio-group v-model="category.status">
            <el-radio :label="1">启用</el-radio>
            <el-radio :label="0">禁用</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogFormVisible = false; resetForm(categoryFormRef)">取消</el-button>
          <el-button type="primary" @click="save(categoryFormRef)">保存</el-button>
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