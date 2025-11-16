import request from '@/utils/request'

export const queryAllApi = () =>
  request({
    url: '/category/all',
    method: 'get'
  })