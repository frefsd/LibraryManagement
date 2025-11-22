import request from '@/utils/request'

//分页查询
export const queryPageApi = (begin, end, name, page, pageSize) =>
  request({
    url: '/book/querypage',
    method: 'get',
    params: { begin, end, name, page, pageSize }
  })

  //新增图书
export const addApi = (data) =>
  request({
    url: '/book/add',
    method: 'post',
    data
  })

  //获取图书信息
export const queryInfoApi = (id) =>
  request({
    url: `/book/queryinfo`,
    method: 'get',
    params: { id }
  })

  //修改图书
export const updateApi = (data) =>
  request({
    url: '/book/update',
    method: 'put',
    data
  })

  //删除图书
export const deleteApi = (id) =>
  request({
    url: `/book/delete`,
    method: 'delete',
    params: { id }
  })

  // 获取可借阅的图书列表（未删除 + 有库存）
  export function getAvailableBooksApi(params) {
  return request({
    url: '/book/available',
    method: 'get',
    params
  })
}