<script setup>
import useEmitter from '@/helper/useEmitter'
import axios from '@axios'
import moment from 'moment'
import { onMounted, ref } from 'vue'
import AddMember from './add-member.vue'

const emit = defineEmits(['refreshData'])
const team = ref({})
const form = ref()
const loading = ref(false)
const dialog = ref(false)
const emitter = useEmitter()
const listUsers = ref([])
const isUpdate = ref(false)
const addMemberRef = ref()

const requireFieldRule = [
  value => {
    if(value) return true
    
    return 'Trường này là bắt buộc!'
  },
]

const submit = async () => {
  const { valid } = await form.value.validate()

  loading.value = true
  
  if(valid){
    dialog.value=false
    if(!isUpdate.value)
      await axios.post('/team/create-team', team.value)
    else
      await axios.put(`/team/update-team?teamId=${team.value.id}`, team.value)
    emitter.emit('showAlert', {
      type: 'success',
      content: 'Success!',
    })
    emit('refreshData')
    loading.value = false
  }
  else{
    emitter.emit('showAlert', {
      type: 'error',
      content: 'Thất bại',
    })
    loading.value = false
    
    return false
  }
}

const getListUsers = async () => {
  try {
    const res= await axios.get('/user/GetAllUsers?pageSize=-1&pageNumber=1')

    listUsers.value = res.data
  } catch (error) {
    
  }
}

const openDialog = item => {
  if(item && item.id){
    team.value = { ...item }
    isUpdate.value = true
  }
  else{
    team.value = {}
    isUpdate.value = false
  }
  dialog.value = true
}

const showDialogAddMember = () => {
  addMemberRef.value.openDialog(team.value.id)
}

onMounted(()=>{
  getListUsers()
})

defineExpose({
  openDialog,
})
</script>

<template>
  <VDialog
    v-model="dialog"
    persistent
    width="1024"
  >
    <VCard>
      <VCardTitle>
        <span class="text-h5">THÔNG TIN ĐỘI NHÓM</span>
      </VCardTitle>
      <VCardText>
        <VContainer>
          <VForm ref="form">
            <VRow>
              <VCol cols="6">
                <VTextField
                  v-model="team.code"
                  :rules="requireFieldRule"
                  label="Code*"
                  :disabled="isUpdate"
                  required
                />
              </VCol>
              <VCol cols="6">
                <VTextField
                  v-model="team.name"
                  :rules="requireFieldRule"
                  label="Tên đội nhóm*"
                  required
                />
              </VCol>
              <VCol cols="12">
                <VTextarea
                  v-model="team.description"
                  :rules="requireFieldRule"
                  label="Mô tả*"
                />
              </VCol>
              <VCol cols="12">
                <VTextarea
                  v-model="team.sologan"
                  :rules="requireFieldRule"
                  label="Slogan*"
                />
              </VCol>
              <VCol cols="6">
                <VSelect
                  v-model="team.truongPhongId"
                  clearable
                  label="Trưởng phòng*"
                  :rules="requireFieldRule"
                  required
                  :items="listUsers"
                  item-value="id"
                  item-title="name"
                />
              </VCol>
            </VRow>
          </VForm>
        </VContainer>
        <small style="color: rgb(243, 118, 118);">*trường bắt buộc</small>
        <div v-if="isUpdate">
          <p class="mt-5">
            <b>Danh sách thành viên</b>
          </p>
          <VCard
            variant="outlined"
            style="padding: 10px;"
          >
            <VBtn @click="showDialogAddMember">
              Thêm
            </VBtn>
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
                    Họ tên
                  </th>
                  <th class="text-left">
                    Ngày sinh
                  </th>
                  <th class="text-left">
                    Số điện thoại
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(item, index) in team.users"
                  :key="index"
                >
                  <td>{{ index + 1 }}</td>
                  <td>{{ item.userName }}</td>
                  <td>{{ item.name }}</td>
                  <td>{{ moment(item.dateOfBirth).format('DD/MM/YYYY') }}</td>
                  <td>{{ item.phoneNumber }}</td>
                </tr>
              </tbody>
            </VTable>
          </VCard>
        </div>
      </VCardText>
      <VCardActions>
        <VSpacer />
        <VBtn
          color="blue-darken-1"
          variant="text"
          @click="dialog = false"
        >
          Đóng
        </VBtn>
        <VBtn
          variant="flat"
          :loading="loading"
          @click="submit"
        >
          Lưu
        </VBtn>
      </VCardActions>
    </VCard>
    <AddMember
      ref="addMemberRef"
      @refreshListMember="getListUsers"
    />
  </VDialog>
</template>
