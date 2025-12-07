import request from '@/utils/request'

// 分页查询
export const queryPageApi = (name, phone, cardNumber, page, pageSize) =>
  request({
    url: '/user/querypage',
    method: 'get',
    params: { name, phone, cardNumber, page, pageSize }
  })

// 获取用户信息
export const queryInfoApi = (id) =>
  request({
    url: '/user/getuser',
    method: 'get',
    params: { id }
  })

// 新增用户
export const addApi = (data) =>
  request({
    url: '/user/add',
    method: 'post',
    data
  })

// 修改用户

export const updateApi = (data) => {
  const { id, ...rest } = data
  return request({
    url: '/user/update',
    method: 'put',
    params: { id },
    data: rest
  })
}

// 删除用户
export const deleteApi = (id) =>
  request({
    url: '/user/delete',
    method: 'delete',
    params: { id }
  })

// 启用/禁用用户
export const changeStatusApi = (id, status) =>
  request({
    url: '/user/status',
    method: 'put',
    data: { status },
    params: { id }
  })