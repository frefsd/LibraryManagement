import request from '@/utils/request'

// 获取图书统计图表数据
export function getBookStatsApi(params) {
  return request({
    url: '/report/book/stats',
    method: 'get',
    params
  })
}

// 获取图书概览数据
export function getBookSummaryApi() {
  return request({
    url: '/report/book/summary',
    method: 'get'
  })
}

// 获取分类统计数据
export function getCategoryStatsApi() {
  return request({
    url: '/report/category/stats',
    method: 'get'
  })
}

// 获取借阅统计数据
export function getBorrowStatsApi(params) {
  return request({
    url: '/report/borrow/stats',
    method: 'get',
    params
  })
}