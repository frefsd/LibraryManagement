import request from '@/utils/request'

//获取所有出版社
export const queryAllApi = () => {
  return request({
    url: '/publisher/all',
    method: 'get'
  })
}