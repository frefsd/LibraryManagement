<script setup>
import { onMounted, ref, nextTick } from 'vue'
import * as echarts from 'echarts'
import { getBookStatsApi, getBookSummaryApi } from '@/api/report'
import { ElMessage } from 'element-plus'

// 搜索表单
const searchForm = ref({
  dimension: 'category',
  dateRange: []
})

// 图表实例
const chartRef = ref()
let chartInstance = null

// 数据
const chartData = ref([])
const tableData = ref([])
const summaryData = ref({
  totalBooks: 0,
  availableBooks: 0,
  borrowedBooks: 0,
  categoryCount: 0
})

onMounted(() => {
  // 设置默认时间范围为最近5年
  const currentYear = new Date().getFullYear()
  searchForm.value.dateRange = [currentYear - 4, currentYear]
  
  initChart()
  loadSummaryData()
  loadChartData()
})

// 初始化图表
const initChart = () => {
  nextTick(() => {
    if (chartRef.value) {
      chartInstance = echarts.init(chartRef.value)
      // 窗口大小变化时重绘图表
      window.addEventListener('resize', () => {
        if (chartInstance) {
          chartInstance.resize()
        }
      })
    }
  })
}

// 加载概览数据
const loadSummaryData = async () => {
  try {
    const result = await getBookSummaryApi()
    if (result.code) {
      summaryData.value = result.data
    }
  } catch (error) {
    console.error('加载概览数据失败:', error)
    ElMessage.error('加载概览数据失败')
  }
}

// 加载图表数据
const loadChartData = async () => {
  try {
    const params = {
      dimension: searchForm.value.dimension
    }
    
    if (searchForm.value.dimension === 'year' && searchForm.value.dateRange) {
      params.startYear = searchForm.value.dateRange[0]
      params.endYear = searchForm.value.dateRange[1]
    }

    const result = await getBookStatsApi(params)
    if (result.code) {
      chartData.value = result.data
      updateTableData()
      renderChart()
    } else {
      ElMessage.error(result.msg || '加载数据失败')
    }
  } catch (error) {
    console.error('加载图表数据失败:', error)
    ElMessage.error('加载图表数据失败')
  }
}

// 更新表格数据
const updateTableData = () => {
  tableData.value = chartData.value.map(item => ({
    ...item,
    percentage: ((item.count / chartData.value.reduce((sum, curr) => sum + curr.count, 0)) * 100).toFixed(2)
  }))
}

// 渲染图表
const renderChart = () => {
  if (!chartInstance) return

  const option = {
    title: {
      text: getChartTitle(),
      left: 'center',
      textStyle: {
        fontSize: 16,
        fontWeight: 'bold'
      }
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 'left',
      top: 'middle',
      formatter: (name) => {
        const item = chartData.value.find(d => d.name === name)
        return item ? `${name}: ${item.count}本` : name
      }
    },
    series: [
      {
        name: getSeriesName(),
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: true,
          formatter: '{b}: {d}%'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: '14',
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: true
        },
        data: chartData.value.map(item => ({
          value: item.count,
          name: item.name
        }))
      }
    ],
    color: [
      '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de',
      '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#ff9f7f'
    ]
  }

  chartInstance.setOption(option)
}

// 获取图表标题
const getChartTitle = () => {
  const titles = {
    category: '图书分类统计',
    status: '图书状态统计',
    year: '图书出版年份统计'
  }
  return titles[searchForm.value.dimension] || '图书统计'
}

// 获取系列名称
const getSeriesName = () => {
  const names = {
    category: '图书分类',
    status: '图书状态',
    year: '出版年份'
  }
  return names[searchForm.value.dimension] || '图书'
}

// 获取表格列标签
const getTableColumnLabel = () => {
  const labels = {
    category: '分类名称',
    status: '状态',
    year: '年份'
  }
  return labels[searchForm.value.dimension] || '名称'
}

// 维度变化处理
const handleDimensionChange = () => {
  loadChartData()
}

// 时间范围变化处理
const handleDateRangeChange = () => {
  if (searchForm.value.dimension === 'year') {
    loadChartData()
  }
}

// 清空筛选条件
const clear = () => {
  searchForm.value = {
    dimension: 'category',
    dateRange: []
  }
  const currentYear = new Date().getFullYear()
  searchForm.value.dateRange = [currentYear - 4, currentYear]
  loadChartData()
}
</script>


<template>
  <div>
    <div id="title">图书信息统计</div><br>
    
    <!-- 统计条件筛选 -->
    <el-form :inline="true" :model="searchForm" class="demo-form-inline">
      <el-form-item label="统计维度">
        <el-select v-model="searchForm.dimension" placeholder="请选择统计维度" @change="handleDimensionChange">
          <el-option label="按分类统计" value="category"/>
          <el-option label="按状态统计" value="status"/>
          <el-option label="按年份统计" value="year"/>
        </el-select>
      </el-form-item>
      <el-form-item label="时间范围" v-if="searchForm.dimension === 'year'">
        <el-date-picker
          v-model="searchForm.dateRange"
          type="yearrange"
          range-separator="至"
          start-placeholder="开始年份"
          end-placeholder="结束年份"
          value-format="YYYY"
          @change="handleDateRangeChange"
        />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="loadChartData">查询</el-button>
        <el-button type="info" @click="clear">清空</el-button>
      </el-form-item>
    </el-form>

    <!-- 统计图表区域 -->
    <el-row :gutter="20">
      <el-col :span="16">
        <div class="chart-container">
          <div ref="chartRef" style="width: 100%; height: 400px;"></div>
        </div>
      </el-col>
      <el-col :span="8">
        <div class="summary-container">
          <el-card class="summary-card">
            <template #header>
              <div class="card-header">
                <span>统计概览</span>
              </div>
            </template>
            <div class="summary-item">
              <div class="summary-label">图书总数</div>
              <div class="summary-value">{{ summaryData.totalBooks }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">在库图书</div>
              <div class="summary-value">{{ summaryData.availableBooks }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">借出图书</div>
              <div class="summary-value">{{ summaryData.borrowedBooks }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">分类数量</div>
              <div class="summary-value">{{ summaryData.categoryCount }}</div>
            </div>
          </el-card>
        </div>
      </el-col>
    </el-row>

    <!-- 数据表格 -->
    <el-card style="margin-top: 20px;">
      <template #header>
        <div class="card-header">
          <span>详细数据</span>
        </div>
      </template>
      <el-table :data="tableData" border style="width: 100%">
        <el-table-column type="index" label="序号" width="60" align="center"/>
        <el-table-column prop="name" :label="getTableColumnLabel()" align="center"/>
        <el-table-column prop="count" label="图书数量" align="center"/>
        <el-table-column prop="percentage" label="占比" align="center">
          <template #default="scope">
            {{ scope.row.percentage }}%
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>


<style scoped>
#title {
  font-size: 20px;
  font-weight: 600;
}

.chart-container {
  background: #fff;
  padding: 20px;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.summary-container {
  height: 100%;
}

.summary-card {
  height: 400px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.summary-label {
  font-size: 14px;
  color: #666;
}

.summary-value {
  font-size: 18px;
  font-weight: bold;
  color: #409EFF;
}
</style>