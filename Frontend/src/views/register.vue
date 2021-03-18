<template>
	<h1>Register</h1>
	<div class="form-group">
		<label>
			Token:
			<input ref="token" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Username:
			<input ref="login" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Password:
			<input ref="password" type="password" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Register</button>
	<router-link to="/login" class="btn btn-secondary ml-2">Login</router-link>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';

export default class Register extends Vue {
	tokenInput: HTMLInputElement | undefined;
	loginInput: HTMLInputElement | undefined;
	passwordInput: HTMLInputElement | undefined;

	mounted() {
		this.tokenInput = this.$refs.token as HTMLInputElement;
		this.loginInput = this.$refs.login as HTMLInputElement;
		this.passwordInput = this.$refs.password as HTMLInputElement;
	}

	async onclick() {
		if (!this.tokenInput || !this.loginInput || !this.passwordInput) {
			console.error(
				`invalid setup (tokenInput: ${this.tokenInput}, loginInput: ${this.loginInput}, passwordInput: ${this.passwordInput})`
			);
			return;
		}
		const token = encodeURIComponent(this.tokenInput.value);
		const username = encodeURIComponent(this.loginInput.value);
		const password = encodeURIComponent(this.passwordInput.value);
		const inviteUrl = `api/invite/v1/register?token=${token}&userName=${username}&password=${password}`;
		const inviteResult = await Backend.post(inviteUrl);
		if (inviteResult.ok) {
			await router.push('/');
		} else {
			alert('Register failed');
		}
	}
}
</script>
