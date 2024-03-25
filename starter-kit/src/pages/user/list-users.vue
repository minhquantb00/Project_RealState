
<script  setup>
import useEmitter from '@/helper/useEmitter'
import ModalRoleUpdate from '@/pages/user/modules/modal-role-update.vue'
import axios from '@axios'
import { onMounted, ref } from 'vue'

const emitter = useEmitter()
const listUsers = ref([])
const roleUpdateRef = ref()
const userDeleteId = ref()
const showConfirmDialog = ref(false)

onMounted(() => {
  getAllUsers()
})

const getAllUsers = async () => {
  const res = await axios.get('/user/GetAllUsers?pageSize=10&pageNumber=1')

  listUsers.value = res.data
}

const showModalUpdateUserRole = user => {
  roleUpdateRef.value.openDialog(user)
}

const deleteTeam = async isConfirm => {
  if(isConfirm) {
    await axios.put(`/user/DeleteUser?userId=${userDeleteId.value}`)

    emitter.emit('showAlert', {
      type: 'success',
      content: 'Success!',
    })
    getAllUsers()
  }
  showConfirmDialog.value = false
}
</script> 


<template>
  <VCard style="padding: 50px">
    <div>
      <VTable fixed-header>
        <thead>
          <tr>
            <th class="text-left">
              #
            </th>
            <th class="text-left">
              Tài khoản
            </th>
            <th class="text-left">
              Quyền hạn
            </th>
            <th class="text-left">
              Thao tác
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(item, index) in listUsers"
            :key="index"
          >
            <td>{{ index + 1 }}</td>
            <td>{{ item.userName }}</td>
            <td>{{ item.roleName }}</td>
            <td>
              <div style="display: flex">
                <VBtn
                  style="margin-right: 10px"
                  variant="text"
                  size="small"
                  @click="showModalUpdateUserRole(item)"
                >
                  Cập nhật quyền hạn
                </VBtn>
                <VBtn
                  variant="text"
                  size="small"
                  color="#bc2f2f"
                  @click="() => {
                    showConfirmDialog = true;
                    userDeleteId = item.id;
                  }"
                >
                  Xóa
                </VBtn>
              </div>
            </td>
          </tr>
        </tbody>
      </VTable>
    </div>
    <ConfirmDialog
      :is-dialog-visible="showConfirmDialog"
      confirmation-question="Bạn có chắc chắn muốn xóa bản ghi này?"
      confirm-title="Success"
      @confirm="deleteTeam"
    />
    <ModalRoleUpdate
      ref="roleUpdateRef"
      @refresh-list-users="getAllUsers"
    />
  </VCard>
</template>