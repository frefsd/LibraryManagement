import request from '@/utils/request'

//查询所有分类
export const queryAllApi = () =>
  request({
    url: '/category/queryall',
    method: 'get'
  })


//根据id获取分类
export const queryInfoApi = (id) =>
  request({
    url: `/category/queryinfo`,
    method: 'get',
    params: { id }
  })

//分页查询
export const queryPageApi = (page, pageSize) => {
  return request({
    url: '/category/querypage',
    method: 'get',
    params: { page, pageSize }
  })
}

//添加分类
export const addApi = (data) => {
  return request({
    url: '/category/add',
    method: 'post',
    data
  })
}

//更新分类
export const updateApi = (id, data) => {
  return request({
    url: '/category/update',
    method: 'put',
    params: { id },
    data
  })
}

//删除分类
export const deleteApi = (id) => {
  return request({
    url: '/category/delete',
    method: 'delete',
    params: { id }
  })
}