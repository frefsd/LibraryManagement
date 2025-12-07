<script setup>
import { onMounted, ref, nextTick } from 'vue'
import * as echarts from 'echarts'
import { getCategoryStatsApi } from '@/api/report'
import { ElMessage } from 'element-plus'

// 搜索表单
const searchForm = ref({
  type: 'bookCount'
})

// 图表实例
const chartRef = ref()
let chartInstance = null

// 数据
const chartData = ref([])
const tableData = ref([])
const summaryData = ref({
  totalCategories: 0,
  enabledCategories: 0,
  maxBooksCategory: '',
  avgBooksPerCategory: 0
})

onMounted(() => {
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
    const result = await getCategoryStatsApi(searchForm.value.type)
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
  const isBookCount = searchForm.value.type === 'bookCount'

  //先拷贝原始数据
  const data = chartData.value.map(item => ({ ...item }))

  //按当前type排序，从大到小
  data.sort((a, b) => {
    const valueA = isBookCount ? a.bookCount : a.borrowCount
    const valueB = isBookCount ? b.bookCount : b.borrowCount
    return valueB - valueA //降序排列
  })

  //计算total
  const total = chartData.value.reduce((sum, curr) => sum + (isBookCount ? curr.bookCount : curr.borrowCount), 0)

  tableData.value = data.map(item => ({
    ...item,

    //添加percentage
    percentage: total > 0 ? (((isBookCount ? item.bookCount : item.borrowCount) / total) * 100).toFixed(2) : '0.00'
  }))
}

// 渲染图表
const renderChart = () => {
  if (!chartInstance) return

  const isBookCount = searchForm.value.type === 'bookCount'

  const option = {
    title: {
      text: isBookCount ? '各分类图书数量统计' : '各分类借阅次数统计',
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
        return item ? `${name}: ${isBookCount ? item.bookCount : item.borrowCount}` : name
      }
    },
    series: [
      {
        name: isBookCount ? '图书数量' : '借阅次数',
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
          value: isBookCount ? item.bookCount : item.borrowCount,
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

// 清空筛选条件
const clear = () => {
  searchForm.value.type = 'bookCount'
  loadChartData()
}
</script>


<template>
  <div>
    <div id="title">图书分类统计</div><br>

    <!-- 统计条件筛选 -->
    <el-form :inline="true" :model="searchForm" class="demo-form-inline">
      <el-form-item label="统计类型">
        <el-select v-model="searchForm.type" placeholder="请选择统计类型" @change="loadChartData">
          <el-option label="图书数量统计" value="bookCount" />
          <el-option label="借阅次数统计" value="borrowCount" />
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
                <span>分类概览</span>
              </div>
            </template>
            <div class="summary-item">
              <div class="summary-label">总分类数</div>
              <div class="summary-value">{{ summaryData.totalCategories }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">启用分类</div>
              <div class="summary-value">{{ summaryData.enabledCategories }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">图书最多分类</div>
              <div class="summary-value">{{ summaryData.maxBooksCategory }}</div>
            </div>
            <div class="summary-item">
              <div class="summary-label">平均图书数</div>
              <div class="summary-value">{{ summaryData.avgBooksPerCategory }}</div>
            </div>
          </el-card>
        </div>
      </el-col>
    </el-row>

    <!-- 数据表格 -->
    <el-card style="margin-top: 20px;">
      <template #header>
        <div class="card-header">
          <span>分类详细数据</span>
        </div>
      </template>
      <el-table :data="tableData" border style="width: 100%">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="name" label="分类名称" align="center" />
        <el-table-column prop="bookCount" label="图书数量" align="center" />
        <el-table-column prop="borrowCount" label="借阅次数" align="center" />
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
  font-size: 16px;
  font-weight: bold;
  color: #409EFF;
}
</style>