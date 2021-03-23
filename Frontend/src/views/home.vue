<template>
	<h1>Home</h1>
	<button :onclick="logout" class="btn btn-secondary">Logout</button>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';

export default class Home extends Vue {
	async logout() {
		await Backend.auth().logout.logoutCreate();
		await router.go(0);
	}
}
</script>
