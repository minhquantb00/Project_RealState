<script  setup>
import useEmitter from '@/helper/useEmitter'
import axios from '@axios'
import { onMounted, ref } from 'vue'


const emit = defineEmits(['refreshListUsers'])
const showDialog = ref(false)
const loading = ref(false)
const form = ref()
const lstRoles = ref([])
const role = ref({})
const emitter = useEmitter()

const requireFieldRule = [
  value => {
    if(value) return true
    
    return 'Trường này là bắt buộc!'
  },
]

const loadListRoles = async () => {
  const res = await axios.get('/user/GetAllRoles')

  lstRoles.value = res.data
}

const onSubmit = async () => {
  const { valid } = await form.value.validate()

  loading.value = true

  if(valid){
    await axios.put(`/auth/ChangeDecentralizationForAdmin`, { ...role.value })

    emitter.emit('showAlert', {
      type: 'success',
      content: 'Success!',
    })
    emit('refreshListUsers')
    showDialog.value=false
    loading.value = false
  }
  else{
    loading.value = false
  
    return false
  }
}

const openDialog = user => {
  role.value = {
    userId: user.id,
    roleId: user.roleId,
  }
  showDialog.value = true
}

onMounted(() => {
  loadListRoles()
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
            v-model="role.roleId"
            :rules="requireFieldRule"
            label="Chọn quyền hạn"
            :items="lstRoles"
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
