// @/api/borrow.js
import request from '@/utils/request'

//分页查询
export const queryPageApi = (page, pageSize) =>
  request({
    url: '/borrow/querypage',
    method: 'get',
    params: { page, pageSize }
  })

  //新增借阅人信息
export const borrowApi = (data) =>
  request({
    url: '/borrow/borrow',
    method: 'post',
    data
  })

  //修改借阅人信息
export const returnApi = (id) =>
  request({
    url: '/borrow/return',
    method: 'post',
    params: { id }
  })