<script  setup>
import useEmitter from '@/helper/useEmitter'
import axios from '@axios'
import { onMounted, ref } from 'vue'


const emit = defineEmits(['refreshListMember'])
const showDialog = ref(false)
const loading = ref(false)
const form = ref()
const lstUsers = ref([])
const memberId = ref()
const teamIdUpdate = ref()
const emitter = useEmitter()

const requireFieldRule = [
  value => {
    if(value) return true
    
    return 'Trường này là bắt buộc!'
  },
]

const loadListUsers = async () => {
  const res = await axios.get('/user/GetAllUsers?pageSize=-1&pageNumber=1')

  lstUsers.value = res.data
}

const onSubmit = async () => {
  const { valid } = await form.value.validate()

  loading.value = true

  if(valid){
    const res = await axios.put(`/user/AddUserInTeam?userId=${memberId.value}&teamId=${teamIdUpdate.value}`)
    if(res.data.status === 400) {
      emitter.emit('showAlert', {
        type: 'success',
        content: 'Success!',
      })
    }
    emit('refreshListMember')
    showDialog.value=false
    loading.value = false
  }
  else{
    loading.value = false
  
    return false
  }
}

const openDialog = teamId => {
  teamIdUpdate.value = teamId
  showDialog.value = true
}

onMounted(() => {
  loadListUsers()
})

defineExpose({
  openDialog,
})
</script>

<template>
  <VDialog
    v-model="showDialog"
    width="500"
  >
    <VCard>
      <VCardText>
        <VForm ref="form">
          <VSelect
            v-model="memberId"
            :rules="requireFieldRule"
            label="Chọn thành viên"
            :items="lstUsers"
            item-value="id"
            item-title="name"
          />
        </VForm>
      </VCardText>
      <VCardActions>
        <VSpacer />
        <VBtn @click="onSubmit">
          OK
        </VBtn>
      </VCardActions>
    </VCard>
  </VDialog>
</template>
