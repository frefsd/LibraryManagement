import request from '@/utils/request'

export const queryAllApi = () => {
  return request({
    url: '/publisher/all',
    method: 'get'
  })
}