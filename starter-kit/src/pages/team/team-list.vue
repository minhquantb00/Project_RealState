
<script  setup>
import useEmitter from '@/helper/useEmitter'
import UpdateTeam from '@/pages/team/modules/update-team.vue'
import axios from '@axios'
import { onMounted, ref } from 'vue'

const emitter = useEmitter()
const listTeam = ref([])
const updateTeamRef = ref()
const teamDeleteId = ref()
const showConfirmDialog = ref(false)

onMounted(() => {
  getAllTeams()
})

const getAllTeams = async () => {
  const res = await axios.get('/team/get-all?pageSize=10&pageNumber=1')

  listTeam.value = res.data.data
}

const showModalUpdateTeam = team => {
  updateTeamRef.value.openDialog(team)
}

const deleteTeam = async isConfirm => {
  if(isConfirm) {
    await axios.delete(`/team/delete-team?teamId=${teamDeleteId.value}`)

    emitter.emit('showAlert', {
      type: 'success',
      content: 'Success!',
    })
    getAllTeams()
  }
  showConfirmDialog.value = false
}
</script> 


<template>
  <VCard style="padding: 50px">
    <div class="d-flex justify-end mb-3">
      <VBtn @click="showModalUpdateTeam(undefined)">
        Tạo mới đội nhóm
      </VBtn>
    </div>
    <div>
      <VTable fixed-header>
        <thead>
          <tr>
            <th class="text-left">
              #
            </th>
            <th class="text-left">
              Tên đội nhóm
            </th>
            <th class="text-left">
              Số lượng thành viên
            </th>
            <th class="text-left">
              Thao tác
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(item, index) in listTeam"
            :key="index"
          >
            <td>{{ index + 1 }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.member }}</td>
            <td>
              <div style="display: flex">
                <VBtn
                  style="margin-right: 10px"
                  variant="text"
                  size="small"
                  @click="showModalUpdateTeam(item)"
                >
                  Cập nhật
                </VBtn>
                <VBtn
                  variant="text"
                  size="small"
                  color="#bc2f2f"
                  @click="() => {
                    showConfirmDialog = true;
                    teamDeleteId = item.id;
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
    <UpdateTeam
      ref="updateTeamRef"
      @refresh-data="getAllTeams"
    />
  </VCard>
</template>