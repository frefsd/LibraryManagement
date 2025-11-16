<!-- src/views/report/BorrowReport.vue -->
<template>
  <div>
    <div id="title">借阅信息统计</div><br>
    
    <!-- 统计条件筛选 -->
    <el-form :inline="true" :model="searchForm" class="demo-form-inline">
      <el-form-item label="统计维度">
        <el-select v-model="searchForm.dimension" placeholder="请选择统计维度" @change="handleDimensionChange">
          <el-option label="按月统计" value="month"/>
          <el-option label="按图书统计" value="book"/>
          <el-option label="按读者统计" value="user"/>
          <el-option label="按分类统计" value="category"/>
        </el-select>
      </el-form-item>
      <el-form-item label="时间范围" v-if="searchForm.dimension === 'month'">
        <el-date-picker
          v-model="searchForm.dateRange"
          type="monthrange"
          range-separator="至"
          start-placeholder="开始月份"
          end-placeholder="结束月份"
          value-format="YYYY-MM"
          @change="handleDateRangeChange"
        />
      </el-form-item>
      <el-form-item label="显示数量" v-if="['book', 'user', 'category'].includes(searchForm.dimension)">
        <el-select v-model="searchForm.limit" placeholder="请选择显示数量" @change="loadChartData">
          <el-option label="前5名" :value="5"/>
          <el-option label="前10名" :value="10"/>
          <el-option label="前20名" :value="20"/>
        </el-select>
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
                <span>借阅概览</span>
              </div>
            </template>
            <div class="summary-item">
              <div class="summary-label">总借阅次数</div>
              <div class="summary-value">{{ summaryData.totalBorrows }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">当前借阅中</div>
              <div class="summary-value">{{ summaryData.currentBorrowing }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">逾期数量</div>
              <div class="summary-value">{{ summaryData.overdueCount }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">平均借阅时长</div>
              <div class="summary-value">{{ summaryData.avgBorrowDays }}天</div>
            </div>
          </el-card>
        </div>
      </el-col>
    </el-row>

    <!-- 数据表格 -->
    <el-card style="margin-top: 20px;">
      <template #header>
        <div class="card-header">
          <span>借阅详细数据</span>
        </div>
      </template>
      <el-table :data="tableData" border style="width: 100%">
        <el-table-column type="index" label="序号" width="60" align="center"/>
        <el-table-column :prop="getTableColumnProp()" :label="getTableColumnLabel()" align="center"/>
        <el-table-column prop="borrowCount" label="借阅次数" align="center"/>
        <el-table-column prop="percentage" label="占比" align="center">
          <template #default="scope">
            {{ scope.row.percentage }}%
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { onMounted, ref, nextTick } from 'vue'
import * as echarts from 'echarts'
import { getBorrowStatsApi } from '@/api/report'
import { ElMessage } from 'element-plus'

// 搜索表单
const searchForm = ref({
  dimension: 'month',
  dateRange: [],
  limit: 10
})

// 图表实例
const chartRef = ref()
let chartInstance = null

// 数据
const chartData = ref([])
const tableData = ref([])
const summaryData = ref({
  totalBorrows: 0,
  currentBorrowing: 0,
  overdueCount: 0,
  avgBorrowDays: 0
})

onMounted(() => {
  // 设置默认时间范围为最近6个月
  const currentDate = new Date()
  const endMonth = currentDate.toISOString().slice(0, 7)
  
  const startDate = new Date(currentDate)
  startDate.setMonth(startDate.getMonth() - 5)
  const startMonth = startDate.toISOString().slice(0, 7)
  
  searchForm.value.dateRange = [startMonth, endMonth]
  
  initChart()
  loadChartData()
})

// 初始化图表
const initChart = () => {
  nextTick(() => {
    if (chartRef.value) {
      chartInstance = echarts.init(chartRef.value)
      window.addEventListener('resize', () => {
        if (chartInstance) {
          chartInstance.resize()
        }
      })
    }
  })
}

// 加载图表数据
const loadChartData = async () => {
  try {
    const params = {
      dimension: searchForm.value.dimension,
      limit: searchForm.value.limit
    }
    
    if (searchForm.value.dimension === 'month' && searchForm.value.dateRange) {
      params.startDate = searchForm.value.dateRange[0]
      params.endDate = searchForm.value.dateRange[1]
    }

    const result = await getBorrowStatsApi(params)
    if (result.code) {
      chartData.value = result.data.stats
      summaryData.value = result.data.summary
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
  const total = chartData.value.reduce((sum, curr) => sum + curr.borrowCount, 0)
  tableData.value = chartData.value.map(item => ({
    ...item,
    percentage: total > 0 ? ((item.borrowCount / total) * 100).toFixed(2) : '0.00'
  }))
}

// 渲染图表
const renderChart = () => {
  if (!chartInstance) return

  const isMonth = searchForm.value.dimension === 'month'
  
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
      trigger: isMonth ? 'axis' : 'item',
      formatter: isMonth 
        ? '{b}<br/>{a}: {c}次'
        : '{a} <br/>{b}: {c}次 ({d}%)'
    },
    legend: isMonth ? {} : {
      orient: 'vertical',
      left: 'left',
      top: 'middle'
    },
    grid: isMonth ? {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    } : {},
    xAxis: isMonth ? {
      type: 'category',
      data: chartData.value.map(item => item.name),
      axisLabel: {
        interval: 0,
        rotate: 30
      }
    } : {},
    yAxis: isMonth ? {
      type: 'value'
    } : {},
    series: [
      {
        name: '借阅次数',
        type: isMonth ? 'line' : 'pie',
        data: isMonth 
          ? chartData.value.map(item => item.borrowCount)
          : chartData.value.map(item => ({
              value: item.borrowCount,
              name: item.name
            })),
        ...(isMonth ? {
          smooth: true,
          lineStyle: {
            width: 3
          },
          itemStyle: {
            borderWidth: 2,
            borderColor: '#fff'
          },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(84, 112, 198, 0.5)' },
              { offset: 1, color: 'rgba(84, 112, 198, 0.1)' }
            ])
          }
        } : {
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
            formatter: '{b}: {c}次'
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
          }
        })
      }
    ],
    color: isMonth 
      ? ['#5470c6']
      : [
          '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de',
          '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#ff9f7f'
        ]
  }

  chartInstance.setOption(option)
}

// 获取图表标题
const getChartTitle = () => {
  const titles = {
    month: '月度借阅统计',
    book: '热门图书借阅统计',
    user: '读者借阅统计',
    category: '分类借阅统计'
  }
  return titles[searchForm.value.dimension] || '借阅统计'
}

// 获取表格列属性
const getTableColumnProp = () => {
  const props = {
    month: 'name',
    book: 'bookName',
    user: 'userName',
    category: 'categoryName'
  }
  return props[searchForm.value.dimension] || 'name'
}

// 获取表格列标签
const getTableColumnLabel = () => {
  const labels = {
    month: '月份',
    book: '图书名称',
    user: '读者姓名',
    category: '分类名称'
  }
  return labels[searchForm.value.dimension] || '名称'
}

// 维度变化处理
const handleDimensionChange = () => {
  // 重置相关参数
  if (searchForm.value.dimension !== 'month') {
    searchForm.value.dateRange = []
  }
  if (!['book', 'user', 'category'].includes(searchForm.value.dimension)) {
    searchForm.value.limit = 10
  }
  
  loadChartData()
}

// 时间范围变化处理
const handleDateRangeChange = () => {
  if (searchForm.value.dimension === 'month') {
    loadChartData()
  }
}

// 清空筛选条件
const clear = () => {
  searchForm.value = {
    dimension: 'month',
    dateRange: [],
    limit: 10
  }
  
  // 设置默认时间范围
  const currentDate = new Date()
  const endMonth = currentDate.toISOString().slice(0, 7)
  
  const startDate = new Date(currentDate)
  startDate.setMonth(startDate.getMonth() - 5)
  const startMonth = startDate.toISOString().slice(0, 7)
  
  searchForm.value.dateRange = [startMonth, endMonth]
  
  loadChartData()
}
</script>

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
  font-size: 16px;
  font-weight: bold;
  color: #409EFF;
}
</style>