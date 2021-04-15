<template>
	<div class="card" style="width: 8rem">
		<div class="card-body">
			<router-link to="/broker/new" class="btn btn-outline-primary btn-block">+</router-link>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';

@Options({
	name: 'AddBrokerCard',
})
export default class AddCurrencyCard extends Vue {}
</script>
