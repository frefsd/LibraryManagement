import request from '@/utils/request'

//分页查询
export const queryPageApi = (params) =>
  request({
    url: '/borrow/querypage',
    method: 'get',
    params
  })

//新增借阅
export const borrowApi = (data) =>
  request({
    url: '/borrow/borrow',
    method: 'post',
    data
  })

//归还图书
export const returnApi = (id) =>
  request({
    url: '/borrow/return',
    method: 'post',
    params: { id }
  })

//续借图书
export const renewApi = (id) =>
  request({
    url: '/borrow/renew',
    method: 'post',
    params: { id }
  })